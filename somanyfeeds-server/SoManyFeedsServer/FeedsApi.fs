module SoManyFeedsServer.FeedsApi

open System
open Suave
open Suave.ServerErrors
open Chiron.Operators
open SoManyFeedsServer.Json
open SoManyFeedsServer.FeedsPersistence


module private Encoders =
    open Chiron
    open Chiron.Operators


    let private feedTypeToString (value : FeedRecordType) : string =
        match value with
        | Atom -> "Atom"
        | Rss -> "Rss"


    let feed (record : FeedRecord) : Json<unit> =
        Json.write "id" record.Id
        *> Json.write "type" (feedTypeToString record.FeedType)
        *> Json.write "name" record.Name
        *> Json.write "url" record.Url


    let error (message : string) : Json<unit> =
        Json.write "error" "An error occured"
        *> Json.write "message" message


let private serverError (message : string) =
    message
        |> serializeObject Encoders.error
        |> INTERNAL_ERROR


let list (listFeeds : unit -> Result<FeedRecord list, string>) : WebPart =
    match listFeeds () with
    | Ok feeds ->
        feeds
            |> serializeList Encoders.feed
            |> Successful.OK
    | Error message ->
        serverError message


let create (createFeed : FeedFields -> Result<FeedRecord, string>) : WebPart =
    Successful.OK "create"


let update (updateFeed : FeedFields -> Result<FeedRecord, string>) : WebPart =
    Successful.OK "update"


let delete (deleteFeed : unit -> Result<unit, string>) : WebPart =
    Successful.OK "delete"
