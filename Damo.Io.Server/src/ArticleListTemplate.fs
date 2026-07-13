module DamoIoServer.ArticleListTemplate

open DamoIoServer.Article
open DamoIoServer.ArticleTemplate

open Giraffe.ViewEngine

[<RequireQualifiedAccess>]
module ArticleListTemplate =
    let render (articles: ArticleRecord list) : XmlNode list =
        articles |> List.map ArticleTemplate.render
