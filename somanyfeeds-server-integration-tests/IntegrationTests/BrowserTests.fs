module ``Browser tests``

open System
open System.Diagnostics
open NUnit.Framework
open System.Threading
open Microsoft.AspNetCore.Hosting


let private tokenSource =
    new CancellationTokenSource()

let private runningOnWindows =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT | PlatformID.Win32S | PlatformID.Win32Windows | PlatformID.WinCE -> true
    | _ -> false

let private startSoManyFeedsServer _ =
    Program
        .webHostBuilder()
        .UseUrls("http://localhost:9090")
        .Build()
        .RunAsync(tokenSource.Token)
        |> ignore

[<SetUp>]
let before() = startSoManyFeedsServer()

[<TearDown>]
let after() = tokenSource.Cancel()

[<Test>]
let tests() =
    executeAllSql
        [ "delete from feeds"
          "delete from users" ]

    let yarn = if runningOnWindows then "yarn.cmd" else "yarn"
    let processInfo = ProcessStartInfo(yarn, "run cypress run")
    use p = Process.Start(processInfo)

    p.WaitForExit()

    if (p.ExitCode <> 0)
        then failwith "Cypress tests failed"
        else ()
