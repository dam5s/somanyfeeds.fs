module SoManyFeedsServer.DataSource

open System
open FSharp.Data.Sql


type FindResult<'T> =
    | Found of 'T
    | NotFound
    | FindError of message:string


[<RequireQualifiedAccess>]
module FindResult =
    let asyncFromAsyncResultOfOption (result : AsyncResult<'T option>) : Async<FindResult<'T>> =
        async {
            let! r = result

            match r with
            | Ok (Some value) -> return Found value
            | Ok None -> return NotFound
            | Error message -> return FindError message
        }


[<Literal>]
let private resolutionPath = __SOURCE_DIRECTORY__ + "/../Libraries"

[<Literal>]
let private defaultConnectionString = "User ID=somanyfeeds;Host=localhost;Port=5432;Database=somanyfeeds_dev;Password=secret"


let private connectionString =
    Env.varDefault "DB_CONNECTION" (always defaultConnectionString)


type private SoManyFeedsDb =
    SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL,
                    defaultConnectionString,
                    ResolutionPath = resolutionPath,
                    UseOptionTypes = true>

type FeedEntity = SoManyFeedsDb.dataContext.``public.feedsEntity``
type UserEntity = SoManyFeedsDb.dataContext.``public.usersEntity``
type ArticleEntity = SoManyFeedsDb.dataContext.``public.articlesEntity``
type FeedJobEntity = SoManyFeedsDb.dataContext.``public.feed_jobsEntity``


type DataContext = AsyncResult<SoManyFeedsDb.dataContext>


let dataContext : DataContext =
    async {
       return unsafeOperation "Get data context" { return fun _ ->
           Environment.SetEnvironmentVariable ("PGTZ", "UTC")
           SoManyFeedsDb.GetDataContext(connectionString)
       }
    }


let dataAccessOperation f =
    asyncResult {
        let! ctx = dataContext

        return! unsafeAsyncOperation "Data access" { return fun _ ->
            f ctx
        }
    }
