module DamoIOFrontend.App

open Elmish
open Elmish.React
open Article
open DamoIOFrontend
open Source
open Page

type Flags =
    { Articles: Article.Json array }

type Model =
    { Articles: Article list
      Page: Page }

type Msg =
    | NavigateTo of path:string
    | NavigateToAndPushState of path:string

let init (path: string) (flags: Flags): Model * Cmd<Msg> =
    let articles =
        flags.Articles
        |> Array.toList
        |> List.map Article.fromJson

    let model =
        { Articles = Article.about :: articles
          Page = Page.fromPath path }

    model, Cmd.none

let private update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | NavigateTo path ->
        { model with Page = Page.fromPath path }, Cmd.none
    | NavigateToAndPushState path ->
        { model with Page = Page.fromPath path }, Nav.pushPath path

open Fable.React
open Fable.React.Props

let private sourceToggleHref selectedSources source =
    Source.all
    |> List.choose (fun s ->
               let selected = List.contains s selectedSources
               let sourceStr = Source.toString s
               if s = source
                   then (if selected then None else Some sourceStr)
                   else (if selected then Some sourceStr else None)
           )
    |> String.concat ","
    |> sprintf "/%s"

let private sourceView model dispatch source =
    let onClick msg = OnClick (fun e -> e.preventDefault(); dispatch msg)
    let selectedSources = Page.sources model.Page
    let path = sourceToggleHref selectedSources source
    let selectedClass =
        if List.contains source selectedSources
            then "selected"
            else ""

    li [ Class selectedClass ]
        [ a [ Href path; onClick (NavigateToAndPushState path) ]
              [ str (Source.toString source)
              ]
        ]

let private articlesToDisplay model =
    match Page.sources model.Page with
    | [] -> [ Article.defaultArticle ]
    | sources -> model.Articles |> List.filter (fun a -> List.contains a.Source sources)


let view (model: Model) (dispatch: Msg -> unit): ReactElement =
    let sourceLinks = Source.all
                      |> List.map (sourceView model dispatch)
    let articlesList = model
                       |> articlesToDisplay
                       |> List.map Article.view

    div []
        [ header [ Id "app-header" ]
              [ section [ Class "content" ]
                    [ Logo.view
                      aside [ Id "app-menu" ]
                          [ ul [] sourceLinks ]
                    ]
              ]
          Logo.view
          section [ Id "app-content"; Class "content" ] articlesList
        ]

open Fable.Core.JsInterop

let private window = Browser.Dom.window

let subscriptions _ =
    let sub dispatch =
        window.onpopstate <- Nav.onPathChanged NavigateTo dispatch

    Cmd.ofSub sub

let private main flags =
    let path = window.document.location.pathname

    (init path, update, view)
    |||> Program.mkProgram
    |> Program.withReactHydrate "damo-io-body"
    |> Program.withSubscription subscriptions
    |> Program.runWith flags

window?DamoIO <- {| StartApp = main |}
