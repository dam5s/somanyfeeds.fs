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
    let log = Logger.logSimple logger

    member this.Error err =
        err.Message
        |> Message.event Error
        |> Message.addExns err.Exceptions
        |> log
        err

    member this.Info msg =
        msg
        |> Message.event Info
        |> log
        msg

#endif
