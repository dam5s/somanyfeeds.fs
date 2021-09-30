[<RequireQualifiedAccess>]
module SoManyFeedsServer.HomePage

open Giraffe
open Fable.React
open Fable.React.Props
open SoManyFeedsFrontend.Components

let private strong text = strong [] [ str text ]
let private link url text = a [ Href url; Target "_blank" ] [ str text ]

let view: HttpHandler =
    [ header [ Class "page" ]
          [ div [ Class "page-content" ]
                [ h2 [] [ str "Home" ]
                  h1 [] [ str "Welcome" ]
                ]
          ]
      div [ Class "main" ]
          [ section []
                [ div [ Class "card" ]
                      [ p [ Class "big-message" ]
                            [ strong "SoManyFeeds"
                              str " is a "
                              strong "feed aggregator"
                              str ". Read the latest articles from multiple subscriptions in a "
                              strong "mobile friendly"
                              str " format."
                            ]
                        p [ Class "big-message" ]
                            [ str "This website is written in "
                              link "https://fsharp.org" "F#"
                              str " both for the backend and the frontend. "
                              str "Source code is all "
                              link "https://github.com/dam5s/somanyfeeds.fs" "available on Github"
                              str ". This version is deployed to "
                              link "https://heroku.com" "Heroku"
                              str "."
                            ]
                      ]
                ]
          ]
    ]
    |> Layout.withTabs Tabs.Home
