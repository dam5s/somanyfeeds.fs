module Server.Articles.Handlers

open Giraffe
open GiraffeViewEngine
open Microsoft.AspNetCore.Http
open System
open Server.Articles.Data
open Server.SourceType


type ViewModel =
    { title : string option
    ; link : string option
    ; content : string
    ; date : string option
    ; source : string
    }


module private Views =
    let listView (articlesJson: string) : XmlNode list =
        [ script [ _src "/app.js" ] []
          script []
            [ rawText "if (!window.location.hash) { window.location.hash = '#About,Social,Blog'; }"
              rawText ("Elm.SoManyFeeds.App.fullscreen({\"articles\":" + articlesJson + "});")
            ]
        ]


let private present (record : Record) : ViewModel =
    let dateMap (s : DateTime) = s.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
    let source =
        match record.Source with
        | About -> "About"
        | Social -> "Social"
        | Code -> "Code"
        | Blog -> "Blog"


    { title = record.Title
    ; link = record.Link
    ; content = record.Content
    ; date = Option.map dateMap record.Date
    ; source = source
    }


let list
    (layout : XmlNode list -> XmlNode)
    (findAllRecords : unit -> Record list) =

    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let records = findAllRecords ()
            let serializer = ctx.GetJsonSerializer()

            let listView = records
                            |> List.sortByDescending (fun r -> Option.defaultValue DateTime.UtcNow r.Date)
                            |> List.map present
                            |> serializer.Serialize
                            |> Views.listView
                            |> layout

            return! htmlView listView next ctx
        }
