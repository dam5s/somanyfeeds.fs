[<RequireQualifiedAccess>]
module SoManyFeedsFrontend.Support.Navigation

open Browser
open Elmish

let pushPath (newPath: string) =
    window.history.pushState({||}, "", newPath)
    Cmd.none

let onPathChanged (msg: string -> 'msg) (dispatch: 'msg -> unit) _ =
    dispatch (msg window.location.pathname)

let goTo (destination: string) =
    window.location.replace destination
    Cmd.none
