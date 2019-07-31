module Program

open System
open System.IO
open Support
open Fake.Core
open Fake.IO


let private allProjects =
    [
      "prelude"
      "damo-io-server" 
      "somanyfeeds" 
      "somanyfeeds-tests" 
      "somanyfeeds-server" 
      "somanyfeeds-server-integration-tests" 
      "feeds-processing"
      "feeds-processing-tests" 
      "frontends"
    ]


let private clean _ =
    allProjects
    |> List.collect (fun p -> [ Path.Combine (p, "bin") ; Path.Combine (p, "obj") ])
    |> List.map Directory.delete
    |> ignore


let private somanyfeedsServerIntegrationTests _ =
    Environment.setEnvironVar "PORT" "9090"
    Environment.setEnvironVar "CONTENT_ROOT" (Path.GetFullPath "somanyfeeds-server")
    Environment.setEnvironVar "DB_CONNECTION" "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_integration_tests"
    DotNet.run "somanyfeeds-server-integration-tests" ()
    ()


let private setupCacheBustingLinks _ =
    let timestamp = DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()
    let timestampPath = "somanyfeeds-server/bin/Release/netcoreapp2.2/publish/Resources/templates/_assets_version.html.liquid"
    Support.writeToFile timestampPath timestamp
    ()


[<EntryPoint>]
let main args =
    use ctxt = fakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    Frontend.loadTasks ()
    Database.loadTasks ()

    Target.create "clean" clean
    Target.create "build" DotNet.build
    Target.create "test" DotNet.test
    Target.create "integration-tests" somanyfeedsServerIntegrationTests

    Target.create "release" (fun _ ->
        DotNet.release "damo-io-server" ()
        DotNet.release "somanyfeeds-server" ()
        setupCacheBustingLinks ()
    )


    "clean" |> dependsOn [ "frontend:clean" ]
    "build" |> dependsOn [ "frontend:build" ]

    "build" |> mustRunAfter "clean"

    "test" |> dependsOn [ "build" ]
    "integration-tests" |> dependsOn [ "build" ]
    "release" |> dependsOn [ "test" ; "integration-tests" ; "build" ; "clean" ]

    Target.runOrDefault "release"

    0
