module DamoIoServer.SoManyFeedsHandler

open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

module private View =
    open Fable.React
    open Fable.React.Props

    let private Content = HTMLAttr.Content

    let render () =
        html [ Lang "en" ]
            [ head []
                  [ meta [ CharSet "utf-8" ]
                    meta [ Name "viewport"; Content "width=device-width" ]
                    link [ Rel "stylesheet"; Type "text/css"; Href "/damo-io.css" ]
                    title [] [ str "damo.io - Damien Le Berrigaud's feeds." ]
                  ]
              body []
                  [ div [ Id "damo-io-body" ] [ App.view model ignore ]
                    script [ Src "/damo-io.js" ] []
                    script [ DangerouslySetInnerHTML { __html = js } ] []
                  ]
            ]
        |> Fable.ReactServer.renderToString


let aboutPage: HttpHandler =
    fun next ctx ->
        task {
            return! htmlString View.render next ctx
        }