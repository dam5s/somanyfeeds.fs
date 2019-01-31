module Program

open Suave
open SoManyFeedsServer


[<EntryPoint>]
let main args =
    args
    |> Array.tryHead
    |> Option.bind (fun name -> Tasks.run name)
    |> Option.defaultWith (fun _ ->
        Async.Start FeedsProcessor.backgroundProcessing
        startWebServer Config.create WebApp.webPart
    )

    0
