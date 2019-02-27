module Program

open Suave
open Suave.Logging
open SoManyFeedsServer


[<EntryPoint>]
let main args =
    LoggingConfig.configure ()

    let logger = Log.create "suave"

    args
    |> Array.tryHead
    |> Option.bind (fun name -> Tasks.run name)
    |> Option.defaultWith (fun _ ->
        Async.Start FeedsProcessor.backgroundProcessing
        startWebServer (WebConfig.create logger) WebApp.webPart
    )

    0
