[<AutoOpen>]
module UnsafeOperation

open System


type private Logs = Logs
let private logger = createLogger<Logs>

module private UnsafeOperation =
    let error description (ex: Exception) =
        let msg = sprintf "%s error: %s" description (ex.Message.Trim ())
                  |> logError logger

        sprintf "Exception details %s" (ex.ToString ())
        |> logError logger
        |> ignore

        Error msg

    let doTry description (func: unit -> 'a): Result<'a, string> =
        try func () |> Ok
        with | ex -> error description ex

    let returnFrom description (func: unit -> Result<'a, string>): Result<'a, string> =
        try func ()
        with | ex -> error description ex

type UnsafeOperationBuilder(description: string) =
    member x.Error(ex: Exception) =
        UnsafeOperation.error description ex
    member x.Return(func: unit -> 'a): Result<'a, string> =
        UnsafeOperation.doTry description func
    member x.ReturnFrom(func: unit -> Result<'a, string>): Result<'a, string> =
        UnsafeOperation.returnFrom description func

let unsafeOperation (description: string) =
    UnsafeOperationBuilder(description)

type UnsafeAsyncOperationBuilder(description: string) =
    member x.Error(ex: Exception): AsyncResult<'T> =
        async {
            return UnsafeOperation.error description ex
        }

    member x.Return(func: unit -> 'a): AsyncResult<'a> =
        async {
            return UnsafeOperation.doTry description func
        }

    member x.ReturnFrom(func: unit -> Result<'a, string>): AsyncResult<'a> =
        async {
            return UnsafeOperation.returnFrom description func
        }

let unsafeAsyncOperation (description: string) =
    UnsafeAsyncOperationBuilder(description)
