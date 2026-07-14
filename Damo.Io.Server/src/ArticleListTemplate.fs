module Damo.Io.Server.ArticleListTemplate

open Giraffe.ViewEngine

open Damo.Io.Server.Article
open Damo.Io.Server.ArticleTemplate

[<RequireQualifiedAccess>]
module ArticleListTemplate =
    let render (articles: ArticleRecord list) : XmlNode list =
        articles |> List.map ArticleTemplate.render
