[<AutoOpen>]
module Logging

open Logary


let private log logger lvl msg =
    msg
    |> Message.event lvl
    |> Logger.logSimple logger

    msg


/// Usage, for a logger for module Foo:
///
/// module Foo
///
/// type private Logs = Logs
/// let private logger = createLogger<Logs>
let createLogger<'a> =
    let t = typeof<'a>
    Log.create(t.DeclaringType)


let logError logger msg =
    log logger Error msg

let logInfo logger msg =
    log logger Info msg
