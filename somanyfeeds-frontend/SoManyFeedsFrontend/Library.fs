module SoManyFeedsFrontend.Library

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open SoManyFeedsFrontend.Applications

let private makeFullScreenApp (init: 'flags -> ('model * Cmd<'msg>)) update view =
    (init, update, view)
    |||> Program.mkProgram
    |> Program.withReactBatched "somanyfeeds-body"

let private startRegistrationApp _ =
    (Registration.init, Registration.update, Registration.view)
    |||> makeFullScreenApp
    |> Program.run

let private startManageApp flags =
    (Manage.init, Manage.update, Manage.view)
    |||> makeFullScreenApp
    |> Program.withSubscription Manage.subscriptions
    |> Program.runWith flags

let private startReadApp flags =
    (Read.init, Read.update, Read.view)
    |||> makeFullScreenApp
    |> Program.withSubscription Read.subscriptions
    |> Program.withConsoleTrace
    |> Program.runWith flags

Browser.Dom.window?SoManyFeeds <- {| StartRegistrationApp = startRegistrationApp
                                     StartManageApp = startManageApp
                                     StartReadApp = startReadApp |}
