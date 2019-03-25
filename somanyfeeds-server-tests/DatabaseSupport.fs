[<AutoOpen>]
module DatabaseSupport

open System
open SoManyFeedsServer


let setTestDbConnectionString _ =
    Environment.SetEnvironmentVariable ("DB_CONNECTION",
                                        "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_tests")


let executeSql (sql : string) =
    DataSource.update DataSource.dataSource sql []
    |> Async.RunSynchronously
    |> ignore


let queryDataContext queryFn =
    asyncResult {
        let! ctx = DataSource.dataContext
        return queryFn ctx |> Seq.map id
    }
    |> Async.RunSynchronously
    |> Result.defaultValue Seq.empty
    |> Seq.toList
