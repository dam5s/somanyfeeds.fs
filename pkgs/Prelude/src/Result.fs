[<RequireQualifiedAccess>]
module Result

let defaultValue other result =
    match result with
    | Ok value -> value
    | Error _ -> other

let toOption result =
    match result with
    | Ok value -> Some value
    | Error _ -> None

let apply (func: Result<'a -> 'b, 'c>) (value: Result<'a, 'c>) : Result<'b, 'c> =
    match func, value with
    | Ok f, Ok a -> Ok(f a)
    | _, Error e -> Error e
    | Error e, _ -> Error e

let onOk func result =
    match result with
    | Ok x ->
        func x
        Ok x
    | Error e -> Error e

let onError func result =
    match result with
    | Ok x -> Ok x
    | Error e ->
        func e
        Error e

module Operators =
    let (<!>) = Result.map
    let (<*>) = apply
