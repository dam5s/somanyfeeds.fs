module SoManyFeedsServer.DataAccess

open System.Data.Common
open Npgsql

let private defaultConnectionString () =
    "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev"

let private connectionString =
    Env.varDefault "DB_CONNECTION" defaultConnectionString


let dataSource _  =
    asyncResult {
        return! unsafeOperation "Database connection" { return fun _ ->
            new NpgsqlConnection (connectionString) :> DbConnection
        }
    }
