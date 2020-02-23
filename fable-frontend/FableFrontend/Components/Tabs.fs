[<RequireQualifiedAccess>]
module FableFrontend.Components.Tabs

open Fable.React
open Fable.React.Props
open FableFrontend.Support

type Tab =
    | Home
    | Read
    | Manage

let private tabName = sprintf "%A"

let private tabPath = tabName >> String.toLowerInvariant >> sprintf "/%s"

let private tabView (currentTab: Tab) (tab: Tab) =
    if tab = currentTab
        then a [ Href (tabPath tab); Class "current" ] [ str (tabName tab) ]
        else Html.link (tabPath tab) (tabName tab)

let view (currentTab: Tab) =
    [ Home; Read; Manage ]
    |> List.map (tabView currentTab)
    |> nav []
