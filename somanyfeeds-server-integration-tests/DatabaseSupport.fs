[<AutoOpen>]
module DatabaseSupport

open Npgsql


let executeSql (sql : string) =
    use connection = new NpgsqlConnection (Env.varRequired "DB_CONNECTION")
    connection.Open ()

    use command = connection.CreateCommand ()
    command.CommandText <- sql
    command.ExecuteNonQuery () |> ignore
