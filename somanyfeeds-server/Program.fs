module Program

open Suave
open SoManyFeeds
open SoManyFeedsServer


[<EntryPoint>]
let main args =
    LoggingConfig.configure ()

    args
    |> Array.tryHead
    |> Option.bind Tasks.run
    |> Option.defaultWith (fun _ ->
        Async.Start FeedsProcessor.backgroundProcessingInfinite
        startWebServer WebConfig.create WebApp.webPart
    )

    0
