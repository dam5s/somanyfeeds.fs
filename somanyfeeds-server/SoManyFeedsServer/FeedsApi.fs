module SoManyFeedsServer.FeedsApi

open Suave
open Chiron.Operators
open SoManyFeedsServer.Json
open SoManyFeedsServer.FeedsPersistence


module Encoders =
    open Chiron

    let feed (record : FeedRecord) : Json<unit> =
        Json.write "id" record.Id
        *> Json.write "name" record.Name
        *> Json.write "url" record.Url


module Decoders =
    open Chiron

    let feedFields (json : Json) : JsonResult<FeedFields> * Json =
        let constructor name url =
            { Name = name; Url = url }

        let decoder =
            constructor
                <!> Json.read "name"
                <*> Json.read "url"

        decoder json


let list (listFeeds : unit -> Result<FeedRecord list, string>) : WebPart =
    match listFeeds () with
    | Ok feeds ->
        feeds
            |> serializeList Encoders.feed
            |> jsonResponse HTTP_200
    | Error message ->
        serverError message


let create (createFeed : FeedFields -> Result<FeedRecord, string>) (fields : FeedFields) : WebPart =
    match createFeed fields with
    | Ok feed ->
        feed
            |> serializeObject Encoders.feed
            |> jsonResponse HTTP_201
    | Error message ->
        serverError message


let update (updateFeed : FeedFields -> Result<FeedRecord, string>) (fields : FeedFields) : WebPart =
    match updateFeed fields with
    | Ok feed ->
        feed
            |> serializeObject Encoders.feed
            |> jsonResponse HTTP_200
    | Error message ->
        serverError message


let delete (deleteFeed : unit -> Result<unit, string>) : WebPart =
    match deleteFeed() with
    | Ok _ ->
        Successful.NO_CONTENT
    | Error message ->
        serverError message
