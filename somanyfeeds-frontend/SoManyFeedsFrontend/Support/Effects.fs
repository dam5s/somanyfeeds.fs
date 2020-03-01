module SoManyFeedsFrontend.Support.Effects

open Elmish
open Browser.Dom
open Fable.Core.JsInterop


let redirectTo destination =
    window.location.assign destination
    Cmd.none
    
let clearFormInputs _ =
    let inputs = document.getElementsByTagName "input"

    for i = 0 to inputs.length - 1 do
        let input = inputs.[i]
        input?value <- ""
        
    Cmd.none
