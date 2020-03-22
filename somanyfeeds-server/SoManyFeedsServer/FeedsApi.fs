module SoManyFeedsServer.FeedsApi

open Giraffe
open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsFrontend.Components.Feed


module Json =
    let feed (record: FeedRecord): Feed.Json =
        { id = record.Id
          name = record.Name
          url = record.Url }

let list (listFeeds: AsyncResult<FeedRecord seq>): HttpHandler =
    listFeeds |> Api.list Json.feed

let create (createFeed: FeedFields -> AsyncResult<FeedRecord>) =
    createFeed |> Api.create Json.feed

let update (updateFeed: FeedFields -> AsyncResult<FeedRecord>) =
    updateFeed |> Api.update Json.feed

let delete (deleteFeed: unit -> AsyncResult<unit>) =
    deleteFeed |> Api.delete
