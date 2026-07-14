module Damo.Io.Server.ArticlesHandler

open System
open Damo.Io.Server.IHttpHandler
open Time

open Damo.Io.Server.LayoutTemplate
open Damo.Io.Server.Article
open Damo.Io.Server.ArticlesRepository
open Damo.Io.Server.ArticleListTemplate

open Giraffe

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
