module Damo.Io.Server.ArticlesHandler

open Giraffe
open System
open Time

open Damo.Io.Server.Article
open Damo.Io.Server.ArticleListTemplate
open Damo.Io.Server.ArticlesRepository
open Damo.Io.Server.IHttpHandler
open Damo.Io.Server.LayoutTemplate

type ListArticlesHandler(articlesRepo: ArticlesRepository, layoutTemplate: LayoutTemplate) =
    interface IHttpHandler with
        member _.Handle(next, ctx) =
            task {
                let! articles = articlesRepo.FindAllAsync()

                let now = Posix.fromDateTimeOffset DateTimeOffset.UtcNow

                let sortedArticles =
                    articles |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)

                let! view = ArticleListTemplate.render sortedArticles |> layoutTemplate.RenderAsync

                return! htmlView view next ctx
            }
