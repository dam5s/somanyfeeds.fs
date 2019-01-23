[<AutoOpen>]
module UnsafeOperation
open System


type UnsafeOperationBuilder(description: string) =
    member x.Error(ex: Exception) =
        let msg = sprintf "%s error: %s" description (ex.Message.Trim ())
        eprintfn "%s" msg
        eprintfn "Exception details %s" (ex.ToString ())
        Error msg

    member x.Return(func: unit -> 'a): Result<'a, string> =
        try Ok <| func ()
        with | ex -> x.Error(ex)

    member x.ReturnFrom(func: unit -> Result<'a, string>): Result<'a, string> =
        try func ()
        with | ex -> x.Error(ex)


let unsafeOperation (description: string) =
    new UnsafeOperationBuilder(description)
