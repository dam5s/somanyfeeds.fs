[<AutoOpen>]
module UnsafeOperation

open System


let private logger = loggerWithName "Prelude.UnsafeOperation"


type UnsafeOperationBuilder(description: string) =
    member x.Error(ex: Exception) =
        let msg = sprintf "%s error: %s" description (ex.Message.Trim ())
                  |> logError logger

        sprintf "Exception details %s" (ex.ToString ())
        |> logError logger
        |> ignore

        Error msg

    member x.Return(func: unit -> 'a): Result<'a, string> =
        try Ok <| func ()
        with | ex -> x.Error(ex)

    member x.ReturnFrom(func: unit -> Result<'a, string>): Result<'a, string> =
        try func ()
        with | ex -> x.Error(ex)


let unsafeOperation (description: string) =
    new UnsafeOperationBuilder(description)
