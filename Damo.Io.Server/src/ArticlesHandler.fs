module DamoIoServer.ArticlesHandler

open System
open Damo.Io.Server.IHttpHandler
open Time

open DamoIoServer.LayoutTemplate
open DamoIoServer.Article
open DamoIoServer.ArticlesRepository
open DamoIoServer.ArticleListTemplate

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
