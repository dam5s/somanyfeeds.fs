[<RequireQualifiedAccess>]
module SoManyFeedsFrontend.Components.Tabs

open Fable.React
open Fable.React.Props
open SoManyFeedsFrontend.Support

type Tab =
    | Home
    | Read
    | Manage

let private tabName = sprintf "%A"

let private tabPath tab =
    match tab with
    | Home -> "/"
    | Read -> "/read"
    | Manage -> "/manage"

let private tabView (currentTab: Tab) (tab: Tab) =
    if tab = currentTab
        then a [ Href (tabPath tab); Class "current" ] [ str (tabName tab) ]
        else Html.link (tabPath tab) (tabName tab)

let view (currentTab: Tab) =
    [ Home; Read; Manage ]
    |> List.map (tabView currentTab)
    |> nav []
