[<RequireQualifiedAccess>]
module Result

let defaultValue other result =
    match result with
    | Ok value -> value
    | Error _ -> other
