module SoManyFeeds.FeedsService

open FeedsDataGateway


let createFeed maxFeeds userId fields: AsyncResult<FeedRecord> =
    asyncResult {
        let! count = FeedsDataGateway.countFeeds userId

        if count >= (int64 maxFeeds)
        then return! AsyncResult.error "Max number of feeds reached"
        else return! FeedsDataGateway.createFeed userId fields
    }
