#if !FABLE_COMPILER

[<AutoOpen>]
module Logging

open Logary

/// Usage, for a logger for module Foo:
///
/// module Foo
///
/// type private Logs = Logs
/// let private logger = Logger<Logs>()

type Logger<'a>() =
    let logger = Log.create (typeof<'a>.DeclaringType)
    let log lvl msg =
        msg
        |> Message.event lvl
        |> Logger.logSimple logger
        msg

    member this.Error msg = log Error msg
    member this.Info msg = log Info msg

#endif
