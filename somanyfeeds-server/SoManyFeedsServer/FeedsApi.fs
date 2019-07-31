module SoManyFeedsServer.FeedsApi

open Suave
open Chiron.Operators
open SoManyFeeds.FeedsDataGateway
open SoManyFeedsServer.Json


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


let list (listFeeds : AsyncResult<FeedRecord seq>) : WebPart =
    fun ctx -> async {
        match! listFeeds with
        | Ok feeds -> return! listResponse HTTP_200 Encoders.feed feeds ctx
        | Error message -> return! serverErrorResponse message ctx
    }


let create (createFeed : FeedFields -> AsyncResult<FeedRecord>) (fields : FeedFields) : WebPart =
    fun ctx -> async {
        match! createFeed fields with
        | Ok feed -> return! objectResponse HTTP_201 Encoders.feed feed ctx
        | Error message -> return! serverErrorResponse message ctx
    }


let update (updateFeed : FeedFields -> AsyncResult<FeedRecord>) (fields : FeedFields) : WebPart =
    fun ctx -> async {
        match! updateFeed fields with
        | Ok feed -> return! objectResponse HTTP_200 Encoders.feed feed ctx
        | Error message -> return! serverErrorResponse message ctx
    }


let delete (deleteFeed : unit -> AsyncResult<unit>) : WebPart =
    fun ctx -> async {
        match! deleteFeed () with
        | Ok _ -> return! Successful.NO_CONTENT ctx
        | Error message -> return! serverErrorResponse message ctx
    }
