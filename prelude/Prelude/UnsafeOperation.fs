[<AutoOpen>]
module UnsafeOperation

open System


type private Logs = Logs
let private logger = Logger<Logs>()

module private UnsafeOperation =
    let error description (ex: Exception) =
        let msg = sprintf "%s error: %s" description (ex.Message.Trim())
        Error.create msg ex

    let doTry description func =
        try func() |> Ok
        with ex -> error description ex

    let returnFrom description func =
        try func()
        with ex -> error description ex

type UnsafeOperationBuilder(description: string) =
    member x.Error(ex: Exception) =
        UnsafeOperation.error description ex
    member x.Return(func) =
        UnsafeOperation.doTry description func
    member x.ReturnFrom(func) =
        UnsafeOperation.returnFrom description func

let unsafeOperation description = UnsafeOperationBuilder(description)

type UnsafeAsyncOperationBuilder(description: string) =
    member x.Error(ex: Exception): AsyncResult<'a> =
        async {
            return UnsafeOperation.error description ex
        }

    member x.Return(func): AsyncResult<'a> =
        async {
            return UnsafeOperation.doTry description func
        }

    member x.ReturnFrom(func): AsyncResult<'a> =
        async {
            return UnsafeOperation.returnFrom description func
        }

let unsafeAsyncOperation description = UnsafeAsyncOperationBuilder(description)
