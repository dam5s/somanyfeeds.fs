module DamoIoServer.ArticlesHandler

open System
open Time

open DamoIoServer.LayoutTemplate
open DamoIoServer.Article
open DamoIoServer.ArticlesRepository
open DamoIoServer.ArticleListTemplate

open Giraffe

let list: HttpHandler =
    fun next ctx ->
        task {
            let articlesRepo = ctx.GetService<ArticlesRepository>()
            let layoutTemplate = ctx.GetService<LayoutTemplate>()

            let! articles = articlesRepo.FindAllAsync()

            let now = Posix.fromDateTimeOffset DateTimeOffset.UtcNow

            let sortedArticles =
                articles |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)

            let! view = ArticleListTemplate.render sortedArticles |> layoutTemplate.RenderAsync

            return! htmlView view next ctx
        }
