[<AutoOpen>]
module Support


let always a _ = a
let curry f a b = f (a, b)
let curry2 f a b c = f (a, b, c)

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

    let contains subString (it: string) =
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
