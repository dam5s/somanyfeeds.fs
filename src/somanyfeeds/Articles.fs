module SoManyFeeds.Articles

open Giraffe
open GiraffeViewEngine
open SoManyFeeds.ArticlesData
open System


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


let private present (record: Record): ViewModel =
    let dateMap (s: DateTime) = s.ToShortDateString()

    { Title = record.Title
    ; Link = record.Link
    ; Content = record.Content
    ; Date = Option.map dateMap record.Date
    ; Source = record.Source
    }


let listHandler layout findAllRecords : HttpHandler =
    let listView =
        findAllRecords()
            |> List.map present
            |> Views.listView

    htmlView (layout [ listView ])
