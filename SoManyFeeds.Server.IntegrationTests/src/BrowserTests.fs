module ``Browser tests``

open System
open System.Diagnostics
open System.Threading
open NUnit.Framework
open SoManyFeedsServer

let private runningOnWindows =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT | PlatformID.Win32S | PlatformID.Win32Windows | PlatformID.WinCE -> true
    | _ -> false

[<SetUp>]
let before() =
    SoManyFeedsTestServer.start()
    TestWebsite.start()

[<TearDown>]
let after() =
    SoManyFeedsTestServer.stop()
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
