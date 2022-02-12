[<RequireQualifiedAccess>]
module SoManyFeedsTestServer

open System.IO
open System.Threading
open SoManyFeedsServer
open Microsoft.AspNetCore.Hosting

let private rootDir () =
    let parent dir =
        Directory.GetParent(dir).FullName

    let rec findRootDir dir =
        if File.Exists(Path.Combine(dir, "SoManyFeeds.sln"))
            then dir
            else findRootDir (parent dir)

    findRootDir (Directory.GetCurrentDirectory())

let private tokenSource =
    new CancellationTokenSource()

let start _ =
    let logary = LoggingConfig.configure()

    Program
        .webHostBuilder(logary)
        .UseContentRoot($"{rootDir()}/SoManyFeeds.Server")
        .UseWebRoot($"{rootDir()}/SoManyFeeds.Server/WebRoot")
        .UseUrls("http://localhost:9090")
        .Build()
        .RunAsync(tokenSource.Token)
        |> ignore

let stop _ =
    tokenSource.Cancel()
