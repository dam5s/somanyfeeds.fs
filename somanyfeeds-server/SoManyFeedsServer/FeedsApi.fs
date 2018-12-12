module SoManyFeedsServer.FeedsApi

open System
open Suave
open Suave.ServerErrors
open SoManyFeedsServer
open SoManyFeedsServer.FeedsPersistence


module private Json =
    open Chiron
    open Chiron.Operators

    let private feedTypeToString (value : FeedRecordType) : string =
        match value with
        | Atom -> "Atom"
        | Rss -> "Rss"


    let private encoder (record : FeedRecord) : Json<unit> =
        Json.write "id" record.Id
        *> Json.write "type" (feedTypeToString record.FeedType)
        *> Json.write "name" record.Name
        *> Json.write "url" record.Url


    let one (record : FeedRecord) : string =
        record
        |> Json.serializeWith encoder
        |> Json.format


    let list (records : FeedRecord list) : string =
        records
        |> List.map (Json.serializeWith encoder)
        |> Json.Array
        |> Json.format


    let error (message : string) : string =
        let encoder m =
            Json.write "error" "An error occured"
            *> Json.write "message" m

        message
            |> Json.serializeWith encoder
            |> Json.format



let serverError (message : string) =
    message
        |> Json.error
        |> INTERNAL_ERROR


let list (listFeeds : unit -> Result<FeedRecord list, string>) : WebPart =
    match listFeeds () with
    | Ok feeds ->
        feeds
            |> Json.list
            |> Successful.OK
    | Error message ->
        serverError message


let create (createFeed : FeedFields -> Result<FeedRecord, string>) : WebPart =
    Successful.OK "create"


let update (updateFeed : FeedFields -> Result<FeedRecord, string>) : WebPart =
    Successful.OK "update"


let delete (deleteFeed : unit -> Result<unit, string>) : WebPart =
    Successful.OK "delete"
