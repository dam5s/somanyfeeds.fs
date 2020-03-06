module SoManyFeedsPersistence.DataSource

open FSharp.Data.Sql
open System


type FindResult<'a> =
    | Found of 'a
    | NotFound
    | FindError of message: string

[<RequireQualifiedAccess>]
module FindResult =
    let asyncFromAsyncResultOfOption (result: AsyncResult<'a option>) =
        async {
            match! result with
            | Ok(Some value) -> return Found value
            | Ok None -> return NotFound
            | Error message -> return FindError message
        }


type ExistsResult = FindResult<unit>

[<RequireQualifiedAccess>]
module ExistsResult =
    let asyncFromAsyncResultOfBoolean (result: AsyncResult<bool>) =
        async {
            match! result with
            | Ok true -> return Found ()
            | Ok false -> return NotFound
            | Error message -> return FindError message
        }


[<Literal>]
let private DefaultConnectionString = "User ID=somanyfeeds;Host=localhost;Port=5432;Database=somanyfeeds_dev;Password=secret"


let private connectionString =
    Env.varDefault "DB_CONNECTION" (always DefaultConnectionString)


type private SoManyFeedsDb =
    SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL,
                    DefaultConnectionString,
                    UseOptionTypes=true>

type FeedEntity = SoManyFeedsDb.dataContext.``public.feedsEntity``
type UserEntity = SoManyFeedsDb.dataContext.``public.usersEntity``
type ArticleEntity = SoManyFeedsDb.dataContext.``public.articlesEntity``
type FeedJobEntity = SoManyFeedsDb.dataContext.``public.feed_jobsEntity``


type DataContext = SoManyFeedsDb.dataContext


let asyncDataContext: AsyncResult<DataContext> =
    async {
       return unsafeOperation "Get data context" { return fun _ ->
           Environment.SetEnvironmentVariable("PGTZ", "UTC")
           SoManyFeedsDb.GetDataContext(connectionString)
       }
    }


let dataAccessOperation f =
    asyncResult {
        let! ctx = asyncDataContext

        return! unsafeAsyncOperation "Data access" { return fun _ ->
            f ctx
        }
    }
