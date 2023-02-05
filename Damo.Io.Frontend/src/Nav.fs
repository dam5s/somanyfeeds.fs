[<RequireQualifiedAccess>]
module DamoIoFrontend.Nav

open Browser
open Elmish

let pushPath (newPath: string) =
    window.history.pushState({||}, "", newPath)
    Cmd.none

let onPathChanged (msg: string -> 'msg) (dispatch: 'msg -> unit) _ =
    dispatch (msg window.location.pathname)
