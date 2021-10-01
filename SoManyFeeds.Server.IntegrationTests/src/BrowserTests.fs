module ``Browser tests``

open System
open System.Diagnostics
open System.Threading
open Microsoft.AspNetCore.Hosting
open NUnit.Framework
open SoManyFeedsServer

module SoManyFeeds =
    let private tokenSource =
        new CancellationTokenSource()

    let start _ =
        let logary = LoggingConfig.configure()

        Program
            .webHostBuilder(logary)
            .UseUrls("http://localhost:9090")
            .Build()
            .RunAsync(tokenSource.Token)
            |> ignore

    let stop _ =
        tokenSource.Cancel()

let private runningOnWindows =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT | PlatformID.Win32S | PlatformID.Win32Windows | PlatformID.WinCE -> true
    | _ -> false

[<SetUp>]
let before() =
    SoManyFeeds.start()
    TestWebsite.start()

[<TearDown>]
let after() =
    SoManyFeeds.stop()
    TestWebsite.stop()

[<Test>]
let tests() =
    executeAllSql
        [ "delete from feeds"
          "delete from users"
          "delete from articles" ]

    let npm = if runningOnWindows then "npm.cmd" else "npm"
    let processInfo = ProcessStartInfo(npm, "run test")
    use p = Process.Start(processInfo)

    p.WaitForExit()

    if (p.ExitCode <> 0)
        then failwith "Cypress tests failed"
        else ()
