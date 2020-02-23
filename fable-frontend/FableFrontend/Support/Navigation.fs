[<RequireQualifiedAccess>]
module FableFrontend.Support.Navigation

open Browser
open Elmish
open Fable.Core.JsInterop

let private emptyObj = createObj []

let pushPath (newPath: string) =
    window.history.pushState(emptyObj, "", newPath)
    Cmd.none

let onPathChanged (msg: string -> 'msg) (dispatch: 'msg -> unit) _ =
    dispatch (msg window.location.pathname)

let goTo (destination: string) =
    window.location.replace destination
    Cmd.none
