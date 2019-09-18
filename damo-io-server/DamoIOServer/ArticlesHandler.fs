module DamoIOServer.ArticlesHandler

open Chiron
open Chiron.Operators
open DamoIOServer.ArticlesDataGateway
open DamoIOServer.Sources
open Suave
open Suave.DotLiquid
open System
open Time


let private source record =
    match record.Source with
    | About -> "About"
    | Social -> "Social"
    | Code -> "Code"
    | Blog -> "Blog"


let private toJson record =
    Json.write "title" record.Title
    *> Json.write "link" record.Link
    *> Json.write "content" record.Content
    *> Json.write "date" (Option.map Posix.milliseconds record.Date)
    *> Json.write "source" (source record)


type ArticlesListViewModel =
    { ArticlesJson: string }


let list findAllRecords ctx =
    async {
        let now =
            Posix.fromDateTimeOffset DateTimeOffset.UtcNow

        let recordsJson =
            findAllRecords()
            |> List.sortByDescending (fun r -> Option.defaultValue now r.Date)
            |> List.map (Json.serializeWith toJson)
            |> Json.Array
            |> Json.format

        return! page "articles-list.html.liquid" { ArticlesJson = recordsJson } ctx
    }
