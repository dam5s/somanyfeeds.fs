module DamoIoServer.ArticlesHandler

open System
open Time

open DamoIoServer.Source
open DamoIoServer.LayoutTemplate
open DamoIoServer.Article
open DamoIoServer.ArticlesRepository
open DamoIoServer.ArticleListTemplate

open Giraffe

let list (findArticlesBySources: ArticlesRepository.FindAllBySources) : HttpHandler =
    fun next ctx ->
        task {
            let now = Posix.fromDateTimeOffset DateTimeOffset.UtcNow

            let articles =
                Source.all
                |> findArticlesBySources
                |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)

            let view = ArticleListTemplate.render articles |> LayoutTemplate.render ctx

            return! htmlView view next ctx
        }
