module Program

open Fake.Core
open Fake.IO
open Support
open System
open System.IO


let private clean _ =
    let findAll name =
        DirectoryInfo(".").GetDirectories(name, SearchOption.AllDirectories)

    Array.append (findAll "bin") (findAll "obj")
    |> Array.map (fun dir -> dir.Delete(true))
    |> ignore


let private somanyfeedsServerIntegrationTests _ =
    Environment.setEnvironVar "PORT" "9090"
    Environment.setEnvironVar "CONTENT_ROOT" (Path.GetFullPath "somanyfeeds-server")
    Environment.setEnvironVar "DB_CONNECTION" "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_integration_tests"
    DotNet.run "somanyfeeds-server-integration-tests" ()
    ()


let private setupCacheBustingLinks _ =
    let timestamp = DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()
    let timestampPaths = [
        "somanyfeeds-server/bin/Release/netcoreapp2.2/publish/Resources/templates/_assets_version.html.liquid"
        "somanyfeeds-giraffe-server/WebRoot/assets.version"
    ]
    timestampPaths
    |> List.map (fun path -> writeToFile path timestamp)
    |> ignore


[<EntryPoint>]
let main args =
    use ctxt = fakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    Frontend.loadTasks()
    Database.loadTasks()

    Target.create "clean" clean
    Target.create "build" (fun _ ->
        setupCacheBustingLinks()
        DotNet.build ()
    )
    Target.create "test" DotNet.test
    Target.create "integration-tests" somanyfeedsServerIntegrationTests

    Target.create "release" (fun _ ->
        DotNet.release "damo-io-server" ()
        DotNet.release "somanyfeeds-server" ()
    )


    "clean" |> dependsOn [ "frontend:clean" ]
    "build" |> dependsOn [ "frontend:build" ]

    "build" |> mustRunAfter "clean"

    "test" |> dependsOn [ "build" ]
    "integration-tests" |> dependsOn [ "build" ]
    "release" |> dependsOn [ "test"; "integration-tests"; "build"; "clean" ]

    Target.runOrDefault "release"

    0
