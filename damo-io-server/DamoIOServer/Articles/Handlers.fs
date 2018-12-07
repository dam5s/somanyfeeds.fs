module DamoIOServer.Articles.Handlers

open System
open Suave
open Suave.DotLiquid
open Chiron
open Chiron.Operators
open DamoIOServer.Sources
open DamoIOServer.Articles.Data


let private epoch: DateTime =
    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)


let private dateMap (d : DateTimeOffset): int64 =
    d.ToUnixTimeMilliseconds ()


let private source (record : Record) : string =
    match record.Source with
    | About -> "About"
    | Social -> "Social"
    | Code -> "Code"
    | Blog -> "Blog"


let private toJson (record : Record): Json<unit> =
    Json.write "title" record.Title
    *> Json.write "link" record.Link
    *> Json.write "content" record.Content
    *> Json.write "date" (Option.map dateMap record.Date)
    *> Json.write "source" (source record)


type ArticlesListViewModel =
    { ArticlesJson : string }


let list (findAllRecords : unit -> Record list) (ctx : HttpContext): Async<HttpContext option> =
    async {
        let records = findAllRecords ()

        let recordsJson = records
                        |> List.sortByDescending (fun r -> Option.defaultValue DateTimeOffset.UtcNow r.Date)
                        |> List.map (Json.serializeWith toJson)
                        |> Json.Array
                        |> Json.format

        return! page "articles-list.html.liquid" { ArticlesJson = recordsJson } ctx
    }
