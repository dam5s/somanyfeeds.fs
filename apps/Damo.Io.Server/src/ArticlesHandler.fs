module Damo.Io.Server.ArticlesHandler

open Damo.Io.Server.ArticleTemplate
open Damo.Io.Server.ListArticles
open Giraffe
open System
open Time

open ApiSupport.IHttpHandler
open Damo.Io.Server.LayoutTemplate
open FeedsPersistence.ArticleRecord
open FeedsPersistence.ArticlesRepository

type ListArticlesHandler(articlesRepo: ArticlesRepository, layoutTemplate: LayoutTemplate) =
    interface IHttpHandler with
        member _.Handle(next, ctx) =
            task {
                let! articles = articlesRepo.FindAllAsync()

                let now = Posix.fromDateTimeOffset DateTimeOffset.UtcNow

                let sortedArticles =
                    articles |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)

                let mergedArticles = ListArticles.mergeConsecutiveArticles now sortedArticles

                let! view = ArticleTemplate.renderList mergedArticles |> layoutTemplate.RenderAsync

                return! htmlView view next ctx
            }
