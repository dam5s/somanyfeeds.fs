[<RequireQualifiedAccess>]
module SoManyFeedsServer.Layout

open Giraffe
open GiraffeViewEngine
open SoManyFeedsServer
open SoManyFeedsServer.CacheBusting

type Tab =
    | Home
    | Read
    | Manage

let tabLink (active: Tab) href tab =
    let activeClass =
        if tab = active
            then "current"
            else ""
    a [ _href href; _class activeClass ] [ encodedText (sprintf "%A" tab) ]

let private headerView (activeTab: Tab option) =
    let tabs =
        match activeTab with
        | None -> []
        | Some tab ->
            [ tabLink tab "/" Home
              tabLink tab "/read" Read
              tabLink tab "/manage" Manage ]

    header [ _class "app-header" ]
        [ div []
              [ a [ _href "/" ] [ Logo.view ]
                nav [] tabs ] ]

let private withoutHeader (content: XmlNode list) =
    html [ _lang "en" ]
        [ head []
              [ meta [ _charset "utf-8" ]
                meta [ _name "viewport"; _content "width=device-width" ]
                link [ _rel "apple-touch-icon"; _sizes "180x180"; _href (assetPath "/apple-touch-icon.png") ]
                link [ _rel "icon"; _type "image/png"; _sizes "32x32"; _href (assetPath "/favicon-32x32.png") ]
                link [ _rel "icon"; _type "image/png"; _sizes "16x16"; _href (assetPath "/favicon-16x16.png") ]
                link [ _rel "manifest"; _href (assetPath "/site.webmanifest") ]
                link [ _rel "mask-icon"; _href (assetPath "/safari-pinned-tab.svg"); attr "color" "#5bbad5" ]
                meta [ _name "msapplication-TileColor"; _content "#002f2f" ]
                meta [ _name "theme-color"; _content "#002f2f" ]
                link [ _rel "stylesheet"; _type "text/css"; _href (assetPath "/somanyfeeds.css") ]
                title [] [ encodedText "SoManyFeeds - A feed aggregator by Damien Le Berrigaud." ]
              ]
          body [] content
        ]

let withoutTabs content =
    withoutHeader
        ([ headerView None ] @ content)

let withTabs tab content =
    withoutHeader
        ([ headerView (Some tab) ] @ content)

let hydrateFableApp view model js =
    withoutHeader
        [ div [ _id "somanyfeeds-body" ] [ rawText (Fable.ReactServer.renderToString (view model ignore)) ]
          script [ _src (assetPath "/somanyfeeds.js") ] []
          script [] [ rawText js ]
        ]
