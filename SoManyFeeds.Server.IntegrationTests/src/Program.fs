module SoManyFeedsIntegrationTests

open FsUnit
open System

type InitMsgUtils() = inherit FSharpCustomMessageFormatter()

let [<EntryPoint>] main _ =
    TestWebsite.start()
    SoManyFeedsTestServer.start()

    printfn "Test servers are now started, press enter to exit"
    Console.ReadLine() |> ignore;

    TestWebsite.stop()
    SoManyFeedsTestServer.stop()
    0
