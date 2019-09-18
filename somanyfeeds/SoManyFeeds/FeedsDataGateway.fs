module SoManyFeeds.FeedsDataGateway

open AsyncResult.Operators
open SoManyFeeds.DataSource


type FeedRecord =
    { Id: int64
      UserId: int64
      Name: string
      Url: string
    }


type FeedFields =
    { Name: string
      Url: string
    }


let private entityToRecord (entity: FeedEntity) =
    { Id = entity.Id
      UserId = entity.UserId
      Name = entity.Name
      Url = entity.Url
    }


let listFeeds userId: AsyncResult<FeedRecord seq> =
    dataAccessOperation (fun ctx ->
        query {
            for feed in ctx.Public.Feeds do
            where (feed.UserId = userId)
        }
        |> Seq.map entityToRecord
    )


let findFeed userId feedId =
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


let countFeeds userId: AsyncResult<int64> =
    dataAccessOperation (fun ctx ->
        query {
            for feed in ctx.Public.Feeds do
            where (feed.UserId = userId)
            count
        }
        |> int64
    )


let createFeed userId fields: AsyncResult<FeedRecord> =
    dataAccessOperation (fun ctx ->
        let entity = ctx.Public.Feeds.Create()
        entity.UserId <- userId
        entity.Name <- fields.Name
        entity.Url <- fields.Url

        ctx.SubmitUpdates()

        entityToRecord entity
    )


let updateFeed userId feedId fields: AsyncResult<FeedRecord> =
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

            ctx.SubmitUpdates()

            entity
        )
    )
    |> AsyncResult.bind entityOptionToAsyncResult


let deleteFeed userId feedId: AsyncResult<unit> =
    dataAccessOperation (fun ctx ->
        query {
            for feed in ctx.Public.Feeds do
            where (feed.Id = feedId && feed.UserId = userId)
            take 1
        }
        |> Seq.tryHead
        |> Option.map (fun entity -> entity.Delete())
        |> ignore

        ctx.SubmitUpdates()
    )
    <!> always()
