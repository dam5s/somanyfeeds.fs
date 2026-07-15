[<AutoOpen>]
module TaskResult

open System.Linq
open System.Runtime.CompilerServices
open System.Threading.Tasks
open Microsoft.Extensions.Logging

type TaskResult<'a> = Task<Result<'a, exn list>>

[<RequireQualifiedAccess>]
module TaskResult =

    let fromValue value : TaskResult<'a> = task { return Ok value }

    let fromResult result : TaskResult<'a> = task { return result }

    let map mapping (result: TaskResult<'a>) : TaskResult<'b> =
        task {
            match! result with
            | Ok value -> return Ok(mapping value)
            | Error err -> return Error err
        }

    let bind (mapping: 'a -> TaskResult<'b>) (result: TaskResult<'a>) : TaskResult<'b> =
        task {
            match! result with
            | Ok value -> return! mapping value
            | Error err -> return Error err
        }

    let mapError mapping (result: TaskResult<'a>) : TaskResult<'a> =
        task {
            match! result with
            | Ok value -> return Ok value
            | Error err -> return Error(mapping err)
        }

    let defaultValue value (result: TaskResult<'a>) =
        task {
            match! result with
            | Ok x -> return x
            | Error _ -> return value
        }

type TaskResultBuilder() =
    member x.Bind(result, mapping) = TaskResult.bind mapping result
    member x.Return(value) = task { return Ok value }
    member x.ReturnFrom(result) = result


let taskResult = TaskResultBuilder()


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
