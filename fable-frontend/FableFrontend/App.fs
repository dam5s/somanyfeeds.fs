module App

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fable.Core.JsInterop

type Model =
    int

type Msg =
    | Increment
    | Decrement

let init () : Model =
    0

let update (msg : Msg) (model : Model) =
    match msg with
    | Increment -> model + 1
    | Decrement -> model - 1

let view (model : Model) dispatch =
  div []
      [ button [ OnClick (fun _ -> dispatch Increment) ] [ str "+" ]
        div [] [ str (string model) ]
        button [ OnClick (fun _ -> dispatch Decrement) ] [ str "-" ]
      ]

let start elementId =
    Program.mkSimple init update view
    |> Program.withReact elementId
    |> Program.withConsoleTrace
    |> Program.run

Browser.window?StartApp <- start
