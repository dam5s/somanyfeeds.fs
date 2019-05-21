module SoManyFeeds.FeedsService

open DataSource
open FeedsDataGateway


let createFeed (maxFeeds : int) (userId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    asyncResult {
        let! count = FeedsDataGateway.countFeeds userId

        if count >= (int64 maxFeeds)
        then return! AsyncResult.error "Max number of feeds reached"
        else return! FeedsDataGateway.createFeed userId fields
    }
