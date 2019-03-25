module SoManyFeedsServer.FeedsDataGateway

open SoManyFeedsServer.DataSource
open AsyncResult.Operators


type FeedRecord =
    { Id : int64
      UserId : int64
      Name : string
      Url : string
    }


type FeedFields =
    { Name : string
      Url : string
    }


let private entityToRecord (entity : FeedEntity) : FeedRecord =
    { Id = entity.Id
      UserId = entity.UserId
      Name = entity.Name
      Url = entity.Url
    }


let listFeeds (userId : int64) : AsyncResult<FeedRecord seq> =
    dataAccessOperation (fun ctx ->
        query {
            for feed in ctx.Public.Feeds do
            where (feed.UserId = userId)
        }
        |> Seq.map entityToRecord
    )


let findFeed (userId : int64) (feedId : int64) : Async<FindResult<FeedRecord>> =
    dataAccessOperation (fun ctx ->
        query {
            for feed in ctx.Public.Feeds do
            where (feed.UserId = userId && feed.Id = feedId)
            take 1
        }
        |> Seq.tryHead
        |> Option.map entityToRecord
    )
    |> FindResult.asyncFromAsyncResultOfOption


let countFeeds (userId : int64) : AsyncResult<int64> =
    dataAccessOperation (fun ctx ->
        query {
            for feed in ctx.Public.Feeds do
            where (feed.UserId = userId)
            count
        }
        |> int64
    )


let createFeed (userId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    dataAccessOperation (fun ctx ->
        let entity = ctx.Public.Feeds.Create ()
        entity.UserId <- userId
        entity.Name <- fields.Name
        entity.Url <- fields.Url

        ctx.SubmitUpdates ()

        entityToRecord entity
    )


let updateFeed (userId : int64) (feedId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    let entityOptionToAsyncResult result =
        match result with
        | Some e -> AsyncResult.result (entityToRecord e)
        | None -> AsyncResult.error "Record not found"

    dataAccessOperation (fun ctx ->
        query {
            for feed in ctx.Public.Feeds do
            where (feed.Id = feedId && feed.UserId = userId)
            take 1
        }
        |> Seq.tryHead
        |> Option.map (fun entity ->
            entity.Name <- fields.Name
            entity.Url <- fields.Url

            ctx.SubmitUpdates ()

            entity
        )
    )
    |> AsyncResult.bind entityOptionToAsyncResult


let deleteFeed (userId : int64) (feedId : int64) : AsyncResult<unit> =
    dataAccessOperation (fun ctx ->
        query {
            for feed in ctx.Public.Feeds do
            where (feed.Id = feedId && feed.UserId = userId)
            take 1
        }
        |> Seq.tryHead
        |> Option.map (fun entity -> entity.Delete ())
        |> ignore

        ctx.SubmitUpdates ()
    )
    <!> always ()
