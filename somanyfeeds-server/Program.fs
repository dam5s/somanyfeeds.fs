module Program

open Suave
open SoManyFeedsServer


[<EntryPoint>]
let main _ =
//    Async.Start FeedsProcessor.backgroundProcessing

    startWebServer Config.create WebApp.webPart
    0
