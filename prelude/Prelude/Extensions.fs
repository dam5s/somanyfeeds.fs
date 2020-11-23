[<AutoOpen>]
module Extensions


let always a _ = a
let curry f a b = f (a, b)
let curry2 f a b c = f (a, b, c)
let tryCast<'a> (a: obj) =
    try Some (a :?> 'a)
    with | _ -> None


[<RequireQualifiedAccess>]
module Option =
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


[<RequireQualifiedAccess>]
module Result =
    let defaultValue other result =
        match result with
        | Ok value -> value
        | Error _ -> other

    let toOption result =
        match result with
        | Ok value -> Some value
        | Error _ -> None

    let apply (func: Result<'a -> 'b, 'c>) (value: Result<'a, 'c>): Result<'b, 'c> =
        match func, value with
        | Ok f, Ok a -> Ok (f a)
        | _, Error e -> Error e
        | Error e, _ -> Error e

    module Operators =
        let (<!>) = Result.map
        let (<*>) = apply


[<RequireQualifiedAccess>]
module Choice =
    let defaultValue other choice =
        match choice with
        | Choice1Of2 value -> value
        | Choice2Of2 _ -> other

    let toOption choice =
        match choice with
        | Choice1Of2 value -> Some value
        | Choice2Of2 _ -> None


[<RequireQualifiedAccess>]
module String =

    let isEmpty value =
        match value with
        | "" -> true
        | _ -> false

    let contains (subString: string) (it: string) =
        it.Contains(subString)

    let equals other (it: string) =
        it.Equals(other)

    let trim (it: string) =
        it.Trim()

    let toLowerInvariant (it: string) =
        it.ToLowerInvariant()

[<RequireQualifiedAccess>]
module List =

    let all predicate list =
        let filtered = List.filter predicate list
        List.length filtered = List.length list

    let updateIf predicate updateFunction =
        List.map (fun elt ->
            if predicate elt
                then updateFunction elt
                else elt
        )

    let updateOne element updateFunction =
        List.map (fun elt ->
            if elt = element
                then updateFunction elt
                else elt
        )
