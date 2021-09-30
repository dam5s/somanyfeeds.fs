module Database

open Evolve
open Evolve.Configuration
open Fake.Core
open Npgsql


let private evolve connection =
    let e = Evolve(connection, fun s -> printfn "%s" s)
    e.IsEraseDisabled <- true
    e.Command <- CommandOptions.Migrate
    e.Locations <- List.toSeq [ "SoManyFeeds.Database/migrations" ]
    e


let private connection connectionString =
    new NpgsqlConnection(connectionString)


let private migrate connectionString =
    connection connectionString
    |> evolve
    |> fun e -> e.Migrate()


let loadTasks _ =
    let localDatabases =
        [
        ("dev", "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev")
        ("tests", "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_tests")
        ("integration tests", "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_integration_tests")
        ]

    Target.create "db:somanyfeeds:local:migrate" (fun _ ->
        localDatabases
        |> List.map (fun (name, connectionString) ->
            printfn $"\n\nMigrating %s{name} database..."
            migrate connectionString
        )
        |> ignore
        printfn "\n"
    )

    Target.create "db:somanyfeeds:remote:migrate" (fun _ ->
        "DB_CONNECTION"
        |> Environment.environVarOrFail
        |> migrate
    )
