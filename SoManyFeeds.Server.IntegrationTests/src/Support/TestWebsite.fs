[<RequireQualifiedAccess>]
module TestWebsite

open System
open System.IO
open System.Threading

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder

let mutable private tokenSource: CancellationTokenSource =
    new CancellationTokenSource()

let start _ =
    let contentRoot = Env.varDefault "FEEDS_CONTENT_ROOT" Directory.GetCurrentDirectory
    let webRoot = Path.Combine(contentRoot, "resources")
    tokenSource <- new CancellationTokenSource()

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

let stop _ =
    tokenSource.Cancel()
