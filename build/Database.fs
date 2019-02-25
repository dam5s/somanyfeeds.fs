module Database

open Evolve
open Evolve.Configuration
open Fake.Core
open Npgsql


let private evolve connection =
    let e = new Evolve (connection, fun s -> printfn "%s" s)
    e.IsEraseDisabled <- true
    e.Command <- CommandOptions.Migrate
    e.Locations <- List.toSeq [ "somanyfeeds-database/migrations" ]
    e


let private connection connectionString =
    new NpgsqlConnection(connectionString)


let private migrate connectionString =
    connection connectionString
    |> evolve
    |> fun e -> e.Migrate()


let loadTasks _ =
    let dev = "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev"
    let test = "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_integration_tests"

    Target.create "db:somanyfeeds:local:migrate" (fun _ ->
        printfn "\n\nMigrating dev database..."
        migrate dev
        printfn "\nMigrating test database..."
        migrate test
        printfn "\n"
    )

    Target.create "db:somanyfeeds:remote:migrate" (fun _ ->
        migrate <| Environment.environVarOrFail "DB_CONNECTION"
    )
