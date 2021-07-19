module SoManyFeedsFrontend.Library

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open SoManyFeedsFrontend.Applications

let private makeFullScreenApp (init: 'flags -> ('model * Cmd<'msg>)) update view =
    (init, update, view)
    |||> Program.mkProgram
    |> Program.withReactHydrate "somanyfeeds-body"

let private startRegistrationApp _ =
    (RegistrationFrontend.init, RegistrationFrontend.update, RegistrationFrontend.view)
    |||> makeFullScreenApp
    |> Program.run

let private startManageApp flags =
    (ManageFrontend.init, ManageFrontend.update, ManageFrontend.view)
    |||> makeFullScreenApp
    |> Program.withSubscription ManageFrontend.subscriptions
    |> Program.runWith flags

let private startReadApp flags =
    (ReadFrontend.init, ReadFrontend.update, ReadFrontend.view)
    |||> makeFullScreenApp
    |> Program.withSubscription ReadFrontend.subscriptions
    |> Program.runWith flags

Browser.Dom.window?SoManyFeeds <- {| StartRegistrationApp = startRegistrationApp
                                     StartManageApp = startManageApp
                                     StartReadApp = startReadApp |}
