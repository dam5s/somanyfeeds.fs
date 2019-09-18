[<AutoOpen>]
module DatabaseSupport

open Npgsql
open SoManyFeeds


let executeSql sql =
    use connection = new NpgsqlConnection(Env.varRequired "DB_CONNECTION")
    connection.Open()

    use command = connection.CreateCommand()
    command.CommandText <- sql
    command.ExecuteNonQuery() |> ignore


let executeAllSql sql =
    sql
    |> List.map executeSql
    |> ignore


let queryDataContext queryFn =
    asyncResult {
        let! ctx = DataSource.asyncDataContext
        return queryFn ctx |> Seq.map id
    }
    |> Async.RunSynchronously
    |> Result.defaultValue Seq.empty
    |> Seq.toList
