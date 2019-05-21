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


[<RequireQualifiedAccess>]
module Env =
    open System

    let var (name : string) : string option =
        Environment.GetEnvironmentVariable(name) |> Option.ofObj

    let varDefault (name : string) (producer : unit -> string) : string =
        var name |> Option.defaultValue (producer ())

[<RequireQualifiedAccess>]
module String =

    let isEmpty (value : string) : bool =
        match value with
        | "" -> true
        | _ -> false

    let contains (subString : string) (it : string) : bool =
        it.Contains(subString)

    let equals (other : string) (it : string) : bool =
        it.Equals(other)

    let trim (it : string) : string =
        it.Trim ()

    let toLowerInvariant (it : string) : string =
        it.ToLowerInvariant ()
