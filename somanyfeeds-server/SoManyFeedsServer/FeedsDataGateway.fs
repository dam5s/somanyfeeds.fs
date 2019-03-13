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


type private FeedEntity = SoManyFeedsDb.dataContext.``public.feedsEntity``


let private entityToRecord (entity : FeedEntity) : FeedRecord =
    { Id = entity.Id
      UserId = entity.UserId
      Name = entity.Name
      Url = entity.Url
    }


let listFeeds (dataContext : DataContext) (userId : int64) : AsyncResult<FeedRecord seq> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            query {
                for feed in ctx.Public.Feeds do
                where (feed.UserId = userId)
            }
            |> Seq.map entityToRecord
        }
    }


let findFeed (dataContext : DataContext) (userId : int64) (feedId : int64) : Async<FindResult<FeedRecord>> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            query {
                for feed in ctx.Public.Feeds do
                where (feed.UserId = userId && feed.Id = feedId)
                take 1
            }
            |> Seq.tryHead
            |> Option.map entityToRecord
        }
    }
    |> FindResult.asyncFromAsyncResultOfOption


let countFeeds (dataContext : DataContext) (userId : int64) : AsyncResult<int64> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            query {
                for feed in ctx.Public.Feeds do
                where (feed.UserId = userId)
                count
            }
            |> int64
        }
    }


let createFeed (dataContext : DataContext) (userId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            let entity = ctx.Public.Feeds.Create ()
            entity.UserId <- userId
            entity.Name <- fields.Name
            entity.Url <- fields.Url

            ctx.SubmitUpdates ()

            entityToRecord entity
        }
    }


let updateFeed (dataContext : DataContext) (userId : int64) (feedId : int64) (fields : FeedFields) : AsyncResult<FeedRecord> =
    let entityOptionToAsyncResult result =
        match result with
        | Some e -> AsyncResult.result (entityToRecord e)
        | None -> AsyncResult.error "Record not found"

    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
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
        }
    }
    |> AsyncResult.bind entityOptionToAsyncResult


let deleteFeed (dataContext : DataContext) (userId : int64) (feedId : int64) : AsyncResult<unit> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            query {
                for feed in ctx.Public.Feeds do
                where (feed.Id = feedId && feed.UserId = userId)
                take 1
            }
            |> Seq.tryHead
            |> Option.map (fun entity -> entity.Delete ())
            |> ignore

            ctx.SubmitUpdates ()
        }
    }
    <!> always ()
