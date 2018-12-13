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


module Decoders =
    open Chiron
    open Chiron.Operators

    let private feedTypeFromString (value : string) : FeedRecordType =
        match value with
        | "Atom" -> Atom
        | _ -> Rss


    let feedFields (json : Json) : JsonResult<FeedFields> * Json =
        let constructor feedType name url =
            { FeedType = feedTypeFromString feedType ; Name = name ; Url = url }

        let decoder =
            constructor
                <!> Json.read "type"
                <*> Json.read "name"
                <*> Json.read "url"

        decoder json


let private serverError (message : string) =
    message
        |> serializeObject Encoders.error
        |> jsonResponse HTTP_500


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
    Successful.OK "delete"
