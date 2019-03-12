module Program

open Suave
open SoManyFeedsServer


[<EntryPoint>]
let main args =
    LoggingConfig.configure ()

    args
    |> Array.tryHead
    |> Option.bind (fun name -> Tasks.run name)
    |> Option.defaultWith (fun _ ->
        Async.Start FeedsProcessor.backgroundProcessing
        startWebServer WebConfig.create WebApp.webPart
    )

    0
