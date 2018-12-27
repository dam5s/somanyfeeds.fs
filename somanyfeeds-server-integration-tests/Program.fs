module SoManyFeedsIntegrationTests

open IntegrationTests
open Suave
open System
open System.Threading
open canopy
open canopy.runner.classic
open canopy.classic
open configuration


let private homeDir : string =
    Environment.GetFolderPath Environment.SpecialFolder.UserProfile

let mutable private tokenSource : CancellationTokenSource option =
    None


once (fun () ->
    let config = SoManyFeedsServer.Config.create
    let webPart = SoManyFeedsServer.App.webPart

    let listening, server = startWebServerAsync config webPart
    let tokenSource = new CancellationTokenSource()

    Async.Start(server, tokenSource.Token)

    listening
        |> Async.RunSynchronously
        |> ignore
)


lastly (fun () ->
    tokenSource
        |> Option.map (fun src -> src.Cancel ())
        |> ignore
)


[<EntryPoint>]
let main (_) =
    chromeDir <- sprintf "%s/dev/chromedriver-2.44" homeDir
    start chrome

    Feeds.all ()

    run()
    quit()

    failedCount
