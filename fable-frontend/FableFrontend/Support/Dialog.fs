module FableFrontend.Support.Dialog

type Dialog<'a> =
    | Initial
    | Opened of 'a
    | Closed

[<RequireQualifiedAccess>]
module Dialog =
    let map (f: 'a -> 'b) dialog =
        match dialog with
        | Opened a -> Opened (f a)
        | Initial -> Initial
        | Closed -> Closed

    let defaultValue other dialog =
        match dialog with
        | Opened a -> a
        | _ -> other
