[<RequireQualifiedAccess>]
module DamoIoServer.LayoutTemplate

open System
open Giraffe.ViewEngine

let render innerTemplate =
    let cssLink name =
        link [ _rel "stylesheet"; _type "text/css"; _href $"/styles/%s{name}" ]

    let disabledMinifiedAssets =
        Environment.GetEnvironmentVariable("DISABLE_MINIFIED_ASSETS") = "true"

    let stylesheets =
        if disabledMinifiedAssets
            then [ cssLink "reset.css"; cssLink "fonts.css"; cssLink "app.css" ]
            else [ cssLink "app.min.css" ]

    html [ _lang "en" ]
        [ head [] ([
            meta [ _charset "utf-8" ]
            meta [ _name "viewport"; _content "width=device-width" ]
            title [] [ str "damo.io - Damien Le Berrigaud's feeds." ]
          ] @ stylesheets)

          body [] [
              div [ _id "template" ] [ innerTemplate ]
              script [ _src "/javascript/htmx-1.8.5.min.js" ] []
          ]
        ]
