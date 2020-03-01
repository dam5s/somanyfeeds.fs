module ``Feeds processing tests``

open System
open System.IO
open System.Threading

open FsUnitTyped.TopLevelOperators
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting

open NUnit.Framework
open SoManyFeedsPersistence.FeedsProcessor


let private tokenSource =
    new CancellationTokenSource()

let private startFeedsServer _ =
    let contentRoot = Env.varDefault "FEEDS_CONTENT_ROOT" Directory.GetCurrentDirectory
    let webRoot = Path.Combine(contentRoot, "Resources")

    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()
        .UseUrls("http://localhost:9092")
        .UseContentRoot(contentRoot)
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> (fun app ->
            app.UseStaticFiles()
            |> ignore
        ))
        .Build()
        .RunAsync(tokenSource.Token)
        |> ignore

[<SetUp>]
let before() = startFeedsServer()

[<TearDown>]
let after() = tokenSource.Cancel()

[<Test>]
let ``test running background jobs``() =
    executeAllSql
        [
        "delete from feed_jobs"
        "delete from articles"
        "delete from feeds"
        "delete from users"
        "insert into users (id, email, name, password_hash) values (10, 'john@example.com', 'Johnny', '')"
        """ insert into feeds (id, user_id, name, url) values
            (101, 10, 'Rss Feed', 'http://localhost:9092/rss.xml'),
            (102, 10, 'Atom Feed', 'http://localhost:9092/atom.xml')
        """
        ]

    Async.RunSynchronously backgroundProcessingOnce

    queryDataContext (fun ctx -> query { for a in ctx.Public.Articles do select a.Id })
    |> Seq.length
    |> shouldEqual 13
