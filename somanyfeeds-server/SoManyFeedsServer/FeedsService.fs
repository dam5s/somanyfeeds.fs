module SoManyFeedsServer.FeedsService

open DataSource
open FeedsDataGateway


let createFeed (dataSource : DataSource) (maxFeeds : int) (userId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    asyncResult {
        let! count = FeedsDataGateway.countFeeds dataSource userId

        if count >= (int64 maxFeeds)
        then
            return! AsyncResult.error "Max number of feeds reached"
        else
            return! FeedsDataGateway.createFeed dataSource userId fields
    }
