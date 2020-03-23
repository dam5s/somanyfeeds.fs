[<RequireQualifiedAccess>]
module SoManyFeedsServer.Layout

open Fable.React
open Fable.React.Props
open SoManyFeedsFrontend.Components
open SoManyFeedsFrontend.Components.Logo
open SoManyFeedsServer.CacheBusting
open Giraffe.ResponseWriters
open SoManyFeedsFrontend.Support

let private headerView (activeTab: Tabs.Tab option) =
    let tabs =
        match activeTab with
        | None -> nav [] []
        | Some tab -> Tabs.view tab

    header [ Class "app-header" ]
        [ div [] [ Logo.view; tabs ] ]

let private withoutHeader (content: ReactElement list) =
    let Color c = HTMLAttr.Custom ("color", c)
    let Content = HTMLAttr.Content

    let view =
        html [ Lang "en" ]
            [ head []
                  [ meta [ CharSet "utf-8" ]
                    meta [ Name "viewport"; Content "width=device-width" ]
                    link [ Rel "apple-touch-icon"; Sizes "180x180"; Href (assetPath "/apple-touch-icon.png") ]
                    link [ Rel "icon"; Type "image/png"; Sizes "32x32"; Href (assetPath "/favicon-32x32.png") ]
                    link [ Rel "icon"; Type "image/png"; Sizes "16x16"; Href (assetPath "/favicon-16x16.png") ]
                    link [ Rel "manifest"; Href (assetPath "/site.webmanifest") ]
                    link [ Rel "mask-icon"; Href (assetPath "/safari-pinned-tab.svg"); Color "#5bbad5" ]
                    meta [ Name "msapplication-TileColor"; Content "#002f2f" ]
                    meta [ Name "theme-color"; Content "#002f2f" ]
                    link [ Rel "stylesheet"; Type "text/css"; Href (assetPath "/somanyfeeds.css") ]
                    title [] [ str "SoManyFeeds - A feed aggregator by Damien Le Berrigaud." ]
                  ]
              body [] content
            ]

    view
    |> Fable.ReactServer.renderToString
    |> htmlString

let withoutTabs content =
    withoutHeader
        ([ headerView None ] @ content)

let withTabs tab content =
    withoutHeader
        ([ headerView (Some tab) ] @ content)

let hydrateFableApp view model js =
    withoutHeader
        [ div [ Id "somanyfeeds-body" ] [ view model ignore ]
          script [ Src (assetPath "/somanyfeeds.js") ] []
          script [ DangerouslySetInnerHTML { __html = js } ] []
        ]
