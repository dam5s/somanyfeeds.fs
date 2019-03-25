[<AutoOpen>]
module DatabaseSupport

open Npgsql
open SoManyFeedsServer


let executeSql (sql : string) =
    use connection = new NpgsqlConnection (Env.varRequired "DB_CONNECTION")
    connection.Open ()

    use command = connection.CreateCommand ()
    command.CommandText <- sql
    command.ExecuteNonQuery () |> ignore


let executeAllSql (sql : string list) =
    sql
    |> List.map executeSql
    |> ignore


let queryDataContext queryFn =
    asyncResult {
        let! ctx = DataSource.dataContext
        return queryFn ctx |> Seq.map id
    }
    |> Async.RunSynchronously
    |> Result.defaultValue Seq.empty
    |> Seq.toList
