module Server.Articles.Handlers

open Giraffe
open GiraffeViewEngine
open Microsoft.AspNetCore.Http
open System
open Server.Articles.Data
open Server.SourceType


type private ViewModel =
    { Title : string option
    ; Link : string option
    ; Content : string
    ; Date : string option
    ; Source : string
    }


module private Views =
    let articleView (article: ViewModel) =
        div [ _class "article" ] <|
            match article.Title with
            | Some title ->
                [ h1 [] [ rawText title ]
                ; p [] [ rawText article.Content ]
                ]
            | None ->
                [ p [] [ rawText article.Content ]
                ]


    let listView (articles: ViewModel list) =
        section [] <| List.map articleView articles


let private present (record : Record) : ViewModel =
    let dateMap (s : DateTime) = s.ToShortDateString()
    let source =
        match record.Source with
        | About -> "About"
        | Social -> "Social"
        | Code -> "Code"
        | Blog -> "Blog"


    { Title = record.Title
    ; Link = record.Link
    ; Content = record.Content
    ; Date = Option.map dateMap record.Date
    ; Source = source
    }


let list
    (layout : XmlNode list -> XmlNode)
    (findAllRecords : unit -> Record list) =

    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let records = findAllRecords ()
            let viewModels = List.map present records
            let listView = Views.listView viewModels

            return! htmlView (layout [ listView ]) next ctx
        }
