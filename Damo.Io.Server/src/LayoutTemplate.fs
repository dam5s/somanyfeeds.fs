module DamoIoServer.LayoutTemplate

open Giraffe
open Giraffe.ViewEngine
open Microsoft.AspNetCore.Http

open DamoIoServer.AssetHashBuilder

[<RequireQualifiedAccess>]
module LayoutTemplate =
    let render (ctx: HttpContext) innerTemplate =
        let hashBuilder = ctx.GetService<AssetHashBuilder>()
        let _assetHref = hashBuilder.Path ctx >> _href

        html
            [ _lang "en" ]
            [ head
                  []
                  [ meta [ _charset "utf-8" ]
                    meta [ _name "viewport"; _content "width=device-width" ]
                    title [] [ str "damo.io - Damien Le Berrigaud's feeds" ]
                    link [ _rel "stylesheet"; _type "text/css"; _assetHref "/styles/app.min.css" ]
                    link [ _rel "icon"; _type "image/svg+xml"; _sizes "any"; _href "/favicon.svg" ] ]
              body [] [ innerTemplate; script [ _src "/javascript/htmx-1.8.5.min.js" ] [] ] ]
