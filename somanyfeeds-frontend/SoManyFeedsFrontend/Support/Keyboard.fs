[<RequireQualifiedAccess>]
module SoManyFeedsFrontend.Support.Keyboard

open Browser.Types

let onEscape msg dispatch (event: KeyboardEvent) =
    match event.key with
    | "Escape"
    | "Esc" -> dispatch msg
    | _ ->
        match event.keyCode with
        | 27.0 -> dispatch msg
        | _ -> ()
