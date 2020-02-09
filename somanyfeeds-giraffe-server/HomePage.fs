[<RequireQualifiedAccess>]
module SoManyFeedsServer.HomePage

open Giraffe
open GiraffeViewEngine

let private txt = rawText
let private strong text = strong [] [ txt text ]
let private link url text = a [ _href url; _target "_blank" ] [ txt text ]

let view =
    [ header [ _class "page" ]
          [ div [ _class "page-content" ]
                [ h2 [] [ txt "Home" ]
                  h1 [] [ txt "Welcome" ]
                ]
          ]
      div [ _class "main" ]
          [ section []
                [ div [ _class "card" ]
                      [ p [ _class "big-message" ]
                            [ strong "SoManyFeeds"
                              txt " is a "
                              strong "feed aggregator"
                              txt ". Read the latest articles from multiple subscriptions in a"
                              strong "mobile friendly"
                              txt " format."
                            ]
                        p [ _class "big-message" ]
                            [ txt "This website is written in "
                              link "https://fsharp.org" "F#"
                              txt " and "
                              link "https://fsharelm-lang.org" "Elm"
                              txt ". Source code is all "
                              link "https://github.com/dam5s/somanyfeeds.fs" "available on Github"
                              txt ". This version is deployed to "
                              link "https://run.pivotal.io" "Pivotal Web Services"
                              txt "."
                            ]
                      ]
                ]
          ]
    ]
    |> Layout.withTabs Layout.Home
