[<RequireQualifiedAccess>]
module Option

let hasValue value opt =
    match opt with
    | Some v -> v = value
    | None -> false
