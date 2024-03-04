module DamoIoServer.ArticlesHandler

open System
open Time
open DamoIoServer.Article
open DamoIoServer.Source
open DamoIoServer.ArticlesRepository

let private sourcesFromPath (path: string) =
    path.Split(",") |> Array.toList |> List.choose Source.tryFromString

open Giraffe
open Giraffe.Htmx

let list (findArticlesBySources: ArticlesRepository.FindAllBySources) path : HttpHandler =
    fun next ctx ->
        task {
            let now = Posix.fromDateTimeOffset DateTimeOffset.UtcNow
            let sources = sourcesFromPath path

            let articles =
                sources
                |> findArticlesBySources
                |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)

            let isHxRequest = ctx.Request.Headers.HxRequest |> Option.defaultValue false

            let articlesView = (articles, sources) ||> ArticleListTemplate.render

            let view =
                if isHxRequest then
                    articlesView
                else
                    LayoutTemplate.render ctx articlesView

            return! htmlView view next ctx
        }
