module DataContext

open System
open FSharp.Data.Sql


[<Literal>]
let private ResolutionPath = __SOURCE_DIRECTORY__ + "/Libraries"

[<Literal>]
let private DefaultConnectionString = "User ID=somanyfeeds;Host=localhost;Port=5432;Database=somanyfeeds_dev;Password=secret"


let private ConnectionString =
    match Environment.GetEnvironmentVariable("DB_CONNECTION") with
    | null -> DefaultConnectionString
    | x -> x


type SoManyFeedsDb =
    SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL, DefaultConnectionString,
                    ResolutionPath = ResolutionPath, Owner="public">


type DataContext = AsyncResult<SoManyFeedsDb.dataContext.publicSchema>


let dataContext : DataContext =
    async {
        return unsafeOperation "DataSource" {
            return fun _ -> SoManyFeedsDb.GetDataContext(ConnectionString).Public
        }
    }
