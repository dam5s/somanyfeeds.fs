[<AutoOpen>]
module Prelude


let always a _ = a


let bindOperation (description : string) (operation : unit -> Result<'T, string>) : Result<'T, string> =
    try operation ()
    with ex ->
        let msg = sprintf "%s error: %s" description (ex.Message.Trim ())
        eprintfn "%s" msg
        eprintfn "Exception details %s" (ex.ToString ())
        Error msg


let tryOperation (description : string) (operation : unit -> 'T) : Result<'T, string> =
    bindOperation description (fun _ -> Ok <| operation ())


module Result =
    let defaultValue (other : 'a) (result : Result<'a, 'b>) : 'a =
        match result with
        | Ok value -> value
        | Error _ -> other

    let toOption (result : Result<'a, 'b>) : 'a option =
        match result with
        | Ok value -> Some value
        | Error _ -> None


module Choice =
    let defaultValue (other : 'a) (choice : Choice<'a, 'b>) : 'a =
        match choice with
        | Choice1Of2 value -> value
        | Choice2Of2 _ -> other
