module WebServerSupport

open Suave
open System.Threading


let start config webPart =
    let listening, server = Web.startWebServerAsync config webPart
    let tokenSource = new CancellationTokenSource()

    Async.Start(server, tokenSource.Token)

    listening
    |> Async.RunSynchronously
    |> ignore

    tokenSource
