module DamoIOServer.ArticlesHandler

open System
open Suave
open Suave.DotLiquid
open Chiron
open Chiron.Operators
open DamoIOServer.Sources
open DamoIOServer.ArticlesDataGateway


let private dateMap (d : DateTimeOffset): int64 =
    d.ToUnixTimeMilliseconds ()


let private source (record : ArticleRecord) : string =
    match record.Source with
    | About -> "About"
    | Social -> "Social"
    | Code -> "Code"
    | Blog -> "Blog"


let private toJson (record : ArticleRecord): Json<unit> =
    Json.write "title" record.Title
    *> Json.write "link" record.Link
    *> Json.write "content" record.Content
    *> Json.write "date" (Option.map dateMap record.Date)
    *> Json.write "source" (source record)


type ArticlesListViewModel =
    { ArticlesJson : string }


let list (findAllRecords : unit -> ArticleRecord list) (ctx : HttpContext): Async<HttpContext option> =
    async {
        let recordsJson =
            findAllRecords ()
            |> List.sortByDescending (fun r -> Option.defaultValue DateTimeOffset.UtcNow r.Date)
            |> List.map (Json.serializeWith toJson)
            |> Json.Array
            |> Json.format

        return! page "articles-list.html.liquid" { ArticlesJson = recordsJson } ctx
    }
