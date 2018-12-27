module Program

open Suave
open SoManyFeedsServer


[<EntryPoint>]
let main _ =
    startWebServer Config.create App.webPart
    0
