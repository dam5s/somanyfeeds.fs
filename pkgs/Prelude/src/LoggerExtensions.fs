module LoggerExtensions

open System.Linq
open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging

type LoggerExtensions() =

    [<Extension>]
    static member LogErrors(logger: ILogger, errors: exn list, msg: string) =
        match errors with
        | [] -> logger.LogError(msg)
        | [ ex ] -> logger.LogError(ex, msg)
        | _ ->
            logger.LogError(msg)

            for index, ex in errors.Index() do
                logger.LogError(ex, "Error #{index}", index)
