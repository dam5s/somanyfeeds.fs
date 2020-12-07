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

    let apply (func: Result<'a -> 'b, 'c>) (value: Result<'a, 'c>): Result<'b, 'c> =
        match func, value with
        | Ok f, Ok a -> Ok (f a)
        | _, Error e -> Error e
        | Error e, _ -> Error e

    module Operators =
        let (<!>) = Result.map
        let (<*>) = apply
