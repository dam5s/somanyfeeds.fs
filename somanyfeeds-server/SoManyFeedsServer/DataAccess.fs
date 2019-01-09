module SoManyFeedsServer.DataAccess

open System.Data.Common
open Npgsql
open SoManyFeedsServer.DataSource

let private defaultConnectionString () =
    "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev"

let private connectionString =
    Env.varDefault "DB_CONNECTION" defaultConnectionString


let dataSource : DataSource =
    fun _ ->
        tryOperation
            "Database Connection"
            (fun _ -> new NpgsqlConnection (connectionString) :> DbConnection)
