[<RequireQualifiedAccess>]
module Option
    let ofBoolean value boolean =
        if boolean
        then Some value
        else None

    let toResult err opt =
        match opt with
        | Some o -> Ok o
        | None -> Error err

    let apply (func: Option<'a -> 'b>) (value: Option<'a>): Option<'b> =
        match func, value with
        | Some f, Some a -> Some (f a)
        | _, _ -> None

    module Operators =
        let (<!>) = Option.map
        let (<*>) = apply
