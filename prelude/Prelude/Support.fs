[<AutoOpen>]
module Support


let always a _ = a


[<RequireQualifiedAccess>]
module Result =
    let defaultValue (other : 'a) (result : Result<'a, 'b>) : 'a =
        match result with
        | Ok value -> value
        | Error _ -> other

    let toOption (result : Result<'a, 'b>) : 'a option =
        match result with
        | Ok value -> Some value
        | Error _ -> None


[<RequireQualifiedAccess>]
module Choice =
    let defaultValue (other : 'a) (choice : Choice<'a, 'b>) : 'a =
        match choice with
        | Choice1Of2 value -> value
        | Choice2Of2 _ -> other

    let toOption (choice : Choice<'a, 'b>) : 'a option =
        match choice with
        | Choice1Of2 value -> Some value
        | Choice2Of2 _ -> None
