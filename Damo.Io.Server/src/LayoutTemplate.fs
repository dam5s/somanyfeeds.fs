[<RequireQualifiedAccess>]
module DamoIoServer.LayoutTemplate

open Giraffe.GiraffeViewEngine

let render innerTemplate =
    html [ _lang "en" ]
        [ head []
              [ meta [ _charset "utf-8" ]
                meta [ _name "viewport"; _content "width=device-width" ]
                link [ _rel "stylesheet"; _type "text/css"; _href "/styles/reset.css" ]
                link [ _rel "stylesheet"; _type "text/css"; _href "/styles/fonts.css" ]
                link [ _rel "stylesheet"; _type "text/css"; _href "/styles/app.css" ]
                title [] [ str "damo.io - Damien Le Berrigaud's feeds." ]
              ]
          body [] [ innerTemplate ]
        ]
