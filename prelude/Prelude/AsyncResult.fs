[<AutoOpen>]
module AsyncResult


type AsyncResult<'a> =
    Async<Result<'a, string>>


[<RequireQualifiedAccess>]
module AsyncResult =

    let result value: AsyncResult<'a> =
        async { return Ok value }

    let error message: AsyncResult<'a> =
        async { return Error message }

    let fromResult result: AsyncResult<'a> =
        async { return result }

    let map mapping (result: AsyncResult<'a>): AsyncResult<'b> =
        async {
            match! result with
            | Ok value -> return Ok(mapping value)
            | Error err -> return Error err
        }

    let bind (mapping: 'a -> AsyncResult<'b>) (result: AsyncResult<'a>): AsyncResult<'b> =
        async {
            match! result with
            | Ok value -> return! mapping value
            | Error err -> return Error err
        }

    let mapError mapping (result: AsyncResult<'a>): AsyncResult<'a> =
        async {
            match! result with
            | Ok value -> return Ok value
            | Error err -> return Error(mapping err)
        }

    let defaultValue value (result: AsyncResult<'a>) =
        async {
            match! result with
            | Ok x -> return x
            | Error _ -> return value
        }

    let using (resource: #System.IDisposable) func =
        try
            func resource
        finally
            resource.Dispose()

    module Operators =
        let (<!>) result mapping = map mapping result


type AsyncResultBuilder() =
    member x.Bind(result, mapping) = AsyncResult.bind mapping result
    member x.Return(value) = async { return Ok value }
    member x.ReturnFrom(result) = result
    member x.Using(resource, func) = AsyncResult.using resource func


let asyncResult = AsyncResultBuilder()
