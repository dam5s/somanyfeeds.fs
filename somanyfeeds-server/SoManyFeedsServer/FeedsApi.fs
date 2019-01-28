module SoManyFeedsServer.FeedsApi

open Suave
open Chiron.Operators
open SoManyFeedsServer.Json
open SoManyFeedsServer.FeedsDataGateway


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


let list (listFeeds : AsyncResult<FeedRecord list>) : WebPart =
    fun ctx -> async {
        match! listFeeds with
        | Ok feeds ->
            return! feeds
            |> serializeList Encoders.feed
            |> jsonResponse HTTP_200
            |> fun wp -> wp ctx

        | Error message ->
            return! serverError message ctx
    }


let create (createFeed : FeedFields -> AsyncResult<FeedRecord>) (fields : FeedFields) : WebPart =
    fun ctx -> async {
        match! createFeed fields with
        | Ok feed ->
            return! feed
            |> serializeObject Encoders.feed
            |> jsonResponse HTTP_201
            |> fun wp -> wp ctx
        | Error message ->
            return! serverError message ctx
    }


let update (updateFeed : FeedFields -> AsyncResult<FeedRecord>) (fields : FeedFields) : WebPart =
    fun ctx -> async {
        match! updateFeed fields with
        | Ok feed ->
            return! feed
            |> serializeObject Encoders.feed
            |> jsonResponse HTTP_200
            |> fun wp -> wp ctx
        | Error message ->
            return! serverError message ctx
    }


let delete (deleteFeed : unit -> AsyncResult<unit>) : WebPart =
    fun ctx -> async {
        match! deleteFeed () with
        | Ok _ ->
            return! Successful.NO_CONTENT ctx
        | Error message ->
            return! serverError message ctx
    }
