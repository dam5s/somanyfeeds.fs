[<RequireQualifiedAccess>]
module DamoIoServer.ArticleListTemplate

open DamoIoServer.Source
open DamoIoServer.Article

let private sourceToggleHref selectedSources source =
    Source.all
    |> List.choose (fun s ->
        let selected = List.contains s selectedSources
        let sourceStr = Source.toString s

        if s = source then
            (if selected then None else Some sourceStr)
        else
            (if selected then Some sourceStr else None)
    )
    |> String.concat ","
    |> sprintf "/%s"

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let private sourceLink selectedSources source =
    let path = sourceToggleHref selectedSources source

    let selectedClass =
        if List.contains source selectedSources then
            "selected"
        else
            ""

    let attrs =
        [ _href path
          _class selectedClass
          _hxGet path
          _hxTrigger "click"
          _hxTarget "#template"
          _hxSwap "outerHTML"
          _hxPushUrl "true" ]

    li [] [ a attrs [ str (Source.toString source) ] ]

let render (articles: ArticleRecord list) (sources: Source list) : XmlNode =
    let sourceLinks = Source.all |> List.map (sourceLink sources)
    let articleList = articles |> List.map ArticleTemplate.render
    let logo = h1 [] [ str "damo.io" ]
    let menu = ul [ _class "main-menu" ] sourceLinks

    div [ _id "template" ] [ aside [] [ logo; menu ]; main [] articleList ]
