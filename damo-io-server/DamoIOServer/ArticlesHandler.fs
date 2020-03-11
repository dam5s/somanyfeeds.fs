module DamoIOServer.ArticlesHandler

open FSharp.Control.Tasks.V2.ContextInsensitive
open DamoIOServer.ArticlesDataGateway
open Giraffe
open System
open Time


let private source record =
    sprintf "%A" record.Source


let private toJson record =
    {| title = record.Title
       link =  record.Link
       content =  record.Content
       date =  (Option.map Posix.milliseconds record.Date)
       source =  (source record)
    |}


type ArticlesListViewModel =
    { ArticlesJson: string }


module View =
    open GiraffeViewEngine

    let render articlesJson =
        html [ _lang "en" ]
            [ head []
                  [ meta [ _charset "utf-8" ]
                    meta [ _name "viewport"; _content "width=device-width" ]
                    link [ _rel "stylesheet"; _type "text/css"; _href "/damo-io.css" ]
                    title [] [ str "damo.io - Damien Le Berrigaud's feed aggregator." ]
                  ]
              body []
                  [ script [ _src "/damo-io.js" ] []
                    script [] [ rawText (sprintf """
                        if (!window.location.hash) {
                            window.location.hash = '#About,Social,Blog';
                        }
                        Elm.DamoIO.App.init({flags: {articles: %s }});
                    """ articlesJson) ]
                  ]
            ]


let list findAllRecords: HttpHandler =
    fun next ctx ->
        task {
            let now =
                Posix.fromDateTimeOffset DateTimeOffset.UtcNow

            let serialize object =
                ctx.GetJsonSerializer().SerializeToString(object)

            let recordsJson =
                findAllRecords()
                |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)
                |> List.map toJson
                |> serialize

            return! htmlView (View.render recordsJson) next ctx
        }
