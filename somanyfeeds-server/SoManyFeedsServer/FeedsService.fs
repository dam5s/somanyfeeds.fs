module SoManyFeedsServer.FeedsService

open DataSource
open FeedsDataGateway


let createFeed (dataSource : DataSource) (maxFeeds : int) (userId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    asyncResult {
        let! count = FeedsDataGateway.countFeeds dataSource userId

        if count >= (int64 maxFeeds)
        then
            return! Error "Max number of feeds reached"
        else
            let! record = FeedsDataGateway.createFeed dataSource userId fields
            return record
    }
