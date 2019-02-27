[<AutoOpen>]
module Logging

open Logary


let private log logger lvl msg =
    msg
    |> Message.event lvl
    |> Logger.logSimple logger

    msg


let getLogger (name : string) =
    Log.create name

let logError logger msg =
    log logger Error msg

let logInfo logger msg =
    log logger Info msg
