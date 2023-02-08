module DamoIoServer.ArticlesHandler

open Giraffe
open System
open Time
open FSharp.Control.Tasks.V2.ContextInsensitive
open DamoIoServer.Article
open DamoIoServer.Source

let private sourcesFromPath (path: string) =
    path.Split(",")
    |> Array.toList
    |> List.choose Source.tryFromString

let list (findArticlesBySources: Source list -> Article list) path: HttpHandler =
    fun next ctx ->
        task {
            let now = Posix.fromDateTimeOffset DateTimeOffset.UtcNow
            let sources = sourcesFromPath path

            let articles =
                sources
                |> findArticlesBySources
                |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)

            let html =
                (articles, sources)
                ||> ArticleListTemplate.render
                |> LayoutTemplate.render
                |> GiraffeViewEngine.renderHtmlDocument

            return! htmlString html next ctx
        }
