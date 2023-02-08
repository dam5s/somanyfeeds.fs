[<RequireQualifiedAccess>]
module DamoIoServer.ArticleListTemplate

open DamoIoServer.Source
open DamoIoServer.Article

let private sourceToggleHref selectedSources source =
    Source.all
    |> List.choose (fun s ->
               let selected = List.contains s selectedSources
               let sourceStr = Source.toString s
               if s = source
                   then (if selected then None else Some sourceStr)
                   else (if selected then Some sourceStr else None)
           )
    |> String.concat ","
    |> sprintf "/%s"

open Giraffe.GiraffeViewEngine

let private sourceLink selectedSources source =
    let path = sourceToggleHref selectedSources source
    let selectedClass =
        if List.contains source selectedSources
            then "selected"
            else ""

    li [ _class selectedClass ]
        [ a [ _href path ] [ str (Source.toString source) ] ]

let render (articles: Article list) (sources: Source list): XmlNode =
    let sourceLinks =
        Source.all
        |> List.map (sourceLink sources)

    let articleList =
        articles
        |> List.map ArticleTemplate.render

    div [ _class "top-level" ]
        [ main [] articleList
          aside [] [ LogoTemplate.render; ul [ _class "main-menu" ] sourceLinks ]
        ]
