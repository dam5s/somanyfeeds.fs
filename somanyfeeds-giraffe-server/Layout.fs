[<RequireQualifiedAccess>]
module SoManyFeedsServer.Layout

open Giraffe
open GiraffeViewEngine

let main (content: XmlNode list) =
    html [ _lang "en" ]
        [ head []
              [ meta [ _charset "utf-8" ]
                meta [ _name "viewport"; _content "width=device-width" ]
                link [ _rel "apple-touch-icon"; _sizes "180x180"; _href "/apple-touch-icon.png" ]
                link [ _rel "icon"; _type "image/png"; _sizes "32x32"; _href "/favicon-32x32.png" ]
                link [ _rel "icon"; _type "image/png"; _sizes "16x16"; _href "/favicon-16x16.png" ]
                link [ _rel "manifest"; _href "/site.webmanifest" ]
                link [ _rel "mask-icon"; _href "/safari-pinned-tab.svg"; attr "color" "#5bbad5" ]
                meta [ _name "msapplication-TileColor"; _content "#002f2f" ]
                meta [ _name "theme-color"; _content "#002f2f" ]
                link [ _rel "stylesheet"; _type "text/css"; _href "/somanyfeeds.css" ]
                title [] [ encodedText "SoManyFeeds - A feed aggregator by Damien Le Berrigaud." ]
              ]
          body [] content
        ]

type Tab =
    | Home
    | Read
    | Manage

let tabLink (active: Tab) href tab =
    let activeClass =
        if tab = active
        then "current"
        else ""
    a [ _href href ; _class activeClass ] [ encodedText (sprintf "%A" tab) ]

let private tabsHeader (active: Tab) =
    header [ _class "app-header" ]
        [ div []
            [ a [_href "/"] [ Logo.view ]
              nav []
                  [ tabLink active "/" Home
                    tabLink active "/read" Read
                    tabLink active "/manage" Manage
                  ]
            ]
        ]

let withTabs (active: Tab) (content: XmlNode list) =
    [ tabsHeader active ] @ content
    |> main
