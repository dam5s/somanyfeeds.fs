[<AutoOpen>]
module AsyncResult


type AsyncResult<'T> =
    Async<Result<'T, string>>


[<RequireQualifiedAccess>]
module AsyncResult =

    let result (value: 'T) : AsyncResult<'T> =
        async { return Ok value }

    let error (message: string) : AsyncResult<'T> =
        async { return Error message }

    let map (mapping : 'T -> 'U) (result : AsyncResult<'T>) : AsyncResult<'U> =
        async {
            match! result with
            | Ok value -> return Ok (mapping value)
            | Error err -> return Error err
        }

    let bind (mapping : 'T -> AsyncResult<'U>) (result : AsyncResult<'T>) : AsyncResult<'U> =
        async {
            match! result with
            | Ok value -> return! mapping value
            | Error err -> return Error err
        }

    let mapError (mapping : string -> string) (result : AsyncResult<'T>) : AsyncResult<'T> =
        async {
            match! result with
            | Ok value -> return Ok value
            | Error err -> return Error (mapping err)
        }

    let defaultValue (value : 'T) (result : AsyncResult<'T>) : Async<'T> =
        async {
            match! result with
            | Ok x -> return x
            | Error _ -> return value
        }

    let using (resource: #System.IDisposable) func =
        try
            func resource
        finally
            resource.Dispose ()

    module Operators =
        let (<!>) result mapping = map mapping result


type AsyncResultBuilder() =
    member x.Bind(result, mapping) = AsyncResult.bind mapping result
    member x.Return(value) = async { return Ok value }
    member x.ReturnFrom(result) = result
    member x.Using(resource, func) = AsyncResult.using resource func


let asyncResult = AsyncResultBuilder()
