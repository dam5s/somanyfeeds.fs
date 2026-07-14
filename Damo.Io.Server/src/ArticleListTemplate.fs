module Damo.Io.Server.ArticleListTemplate

open Damo.Io.Server.Article
open Damo.Io.Server.ArticleTemplate

open Giraffe.ViewEngine

[<RequireQualifiedAccess>]
module ArticleListTemplate =
    let render (articles: ArticleRecord list) : XmlNode list =
        articles |> List.map ArticleTemplate.render
