module FableFrontend.Library

open Elmish
open Elmish.React
open Fable.Core.JsInterop

let private makeFullScreenApp init update view =
    Program.mkProgram init update view
    |> Program.withReactBatched "somanyfeeds-body"

let private startRegistrationApp _ =
    (RegistrationApp.init, RegistrationApp.update, RegistrationApp.view)
    |||> makeFullScreenApp
    |> Program.run

let private startManageApp flags =
    (ManageApp.init, ManageApp.update, ManageApp.view)
    |||> makeFullScreenApp
    |> Program.withSubscription ManageApp.subscriptions
    |> Program.runWith flags

Browser.Dom.window?SoManyFeeds <- {| StartRegistrationApp = startRegistrationApp
                                     StartManageApp = startManageApp |}
