[<RequireQualifiedAccess>]
module SoManyFeedsTestServer

open System
open System.Threading
open SoManyFeedsServer
open Microsoft.AspNetCore.Hosting

let private tokenSource =
    new CancellationTokenSource()

let start _ =
    Environment.SetEnvironmentVariable("DB_CONNECTION", dbConnectionString)
    let contentRoot = projectDir "SoManyFeeds.Server"
    let logary = LoggingConfig.configure()

    Program
        .webHostBuilder("9090", contentRoot, logary)
        .Build()
        .RunAsync(tokenSource.Token)
        |> ignore

let stop _ =
    tokenSource.Cancel()
