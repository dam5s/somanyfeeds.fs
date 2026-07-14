module Damo.Io.Server.LayoutTemplate

open Giraffe.ViewEngine

open Damo.Io.Server.AssetHashBuilder

type LayoutTemplate(hashBuilder: AssetHashBuilder) =
    member _.RenderAsync(innerTemplate) =
        task {
            let! cssPath = hashBuilder.GetPathAsync("/styles/app.min.css")

            return
                html
                    [ _lang "en" ]
                    [ head
                          []
                          [ meta [ _charset "utf-8" ]
                            meta [ _name "viewport"; _content "width=device-width" ]
                            title [] [ str "damo.io - Damien Le Berrigaud's feeds" ]
                            link [ _rel "stylesheet"; _type "text/css"; _href cssPath ]
                            link [ _rel "icon"; _type "image/svg+xml"; _sizes "any"; _href "/favicon.svg" ] ]
                      body [] [ header [] [ h1 [] [ str "damo.io" ] ]; main [] innerTemplate ] ]
        }
