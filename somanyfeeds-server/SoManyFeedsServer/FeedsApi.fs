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
        let! feedsResult = listFeeds

        match feedsResult with
        | Ok feeds -> return! listResponse HTTP_200 Encoders.feed feeds ctx
        | Error message -> return! serverErrorResponse message ctx
    }


let create (createFeed : FeedFields -> AsyncResult<FeedRecord>) (fields : FeedFields) : WebPart =
    fun ctx -> async {
        let! createResult = createFeed fields

        match createResult with
        | Ok feed -> return! objectResponse HTTP_201 Encoders.feed feed ctx
        | Error message -> return! serverErrorResponse message ctx
    }


let update (updateFeed : FeedFields -> AsyncResult<FeedRecord>) (fields : FeedFields) : WebPart =
    fun ctx -> async {
        let! updateResult = updateFeed fields

        match updateResult with
        | Ok feed -> return! objectResponse HTTP_200 Encoders.feed feed ctx
        | Error message -> return! serverErrorResponse message ctx
    }


let delete (deleteFeed : unit -> AsyncResult<unit>) : WebPart =
    fun ctx -> async {
        let! deleteResult = deleteFeed ()

        match deleteResult with
        | Ok _ -> return! Successful.NO_CONTENT ctx
        | Error message -> return! serverErrorResponse message ctx
    }
