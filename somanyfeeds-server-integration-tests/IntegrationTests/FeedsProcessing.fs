module IntegrationTests.FeedsProcessing

open FsUnitTyped.TopLevelOperators
open SoManyFeeds.FeedsProcessor
open Suave
open Suave.Filters
open Suave.Operators
open System.IO
open System.Threading
open canopy.runner.classic


let mutable private tokenSource: CancellationTokenSource option =
    None

let private startFeedsServer _ =
    let binding =
        Http.HttpBinding.createSimple HTTP "0.0.0.0" 9091

    let homeFolder =
        (__SOURCE_DIRECTORY__, "../Resources")
        |> Path.Combine
        |> Path.GetFullPath

    let config =
        { defaultConfig with
            homeFolder = Some homeFolder
            bindings = [ binding ]
        }

    WebServerSupport.start config (GET >=> Files.browseHome)


let all() =
    context "Feeds Processing"

    before (fun _ ->
        tokenSource <- Some <| startFeedsServer()
    )

    after (fun _ ->
        tokenSource
        |> Option.map (fun src -> src.Cancel())
        |> ignore
    )

    "Running background jobs" &&& fun _ ->
        executeAllSql
            [
            "delete from feed_jobs"
            "delete from articles"
            "delete from feeds"
            "delete from users"
            "insert into users (id, email, name, password_hash) values (10, 'john@example.com', 'Johnny', '')"
            """ insert into feeds (id, user_id, name, url) values
                (101, 10, 'Rss Feed', 'http://localhost:9091/rss.xml'),
                (102, 10, 'Atom Feed', 'http://localhost:9091/atom.xml')
            """
            ]


        Async.RunSynchronously backgroundProcessingOnce


        queryDataContext (fun ctx -> query { for a in ctx.Public.Articles do select a.Id })
        |> Seq.length
        |> shouldEqual 13
