﻿module Program

open System
open System.IO
open Fake.Core
open Fake.IO
open Support


let private setupCacheBustingLinks _ =
    let timestamp = DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()
    writeToFile "SoManyFeeds.Server/WebRoot/assets.version" timestamp


[<EntryPoint>]
let main args =
    use ctxt = fakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    Frontend.loadTasks()
    Database.loadTasks()

    Target.create "clean" DotNet.clean
    Target.create "build" (fun _ ->
        setupCacheBustingLinks()
        DotNet.build ()
    )
    Target.create "test" (fun _ ->
        Environment.setEnvironVar "CONTENT_ROOT" (Path.GetFullPath "SoManyFeeds.Server")
        Environment.setEnvironVar "FEEDS_CONTENT_ROOT" (Path.GetFullPath "SoManyFeeds.Server.IntegrationTests")
        Environment.setEnvironVar "DB_CONNECTION" "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_integration_tests"
        DotNet.test ""
    )
    Target.create "integrationTest" (fun _ ->
        Environment.setEnvironVar "CONTENT_ROOT" (Path.GetFullPath "SoManyFeeds.Server")
        Environment.setEnvironVar "FEEDS_CONTENT_ROOT" (Path.GetFullPath "SoManyFeeds.Server.IntegrationTests")
        Environment.setEnvironVar "DB_CONNECTION" "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_integration_tests"
        DotNet.test "SoManyFeeds.Server.IntegrationTests"
    )

    Target.create "release" (fun _ ->
        DotNet.release "Damo.Io.Server" ()
        DotNet.release "SoManyFeeds.Server" ()
    )

    "clean" |> dependsOn [ "frontend:clean" ]
    "build" |> dependsOn [ "frontend:build" ]

    "build" |> mustRunAfter "clean"

    "test" |> dependsOn [ "build" ]
    "release" |> dependsOn [ "test"; "build"; "clean" ]

    Deploy.loadTasks()
    Target.runOrDefault "release"

    0