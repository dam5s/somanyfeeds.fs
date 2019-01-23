[<AutoOpen>]
module Support


let always a _ = a


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
