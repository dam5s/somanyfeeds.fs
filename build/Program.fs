module Program

open System
open System.IO
open Support
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.Runtime


let private dotnet (command : string) (args : string) _ =
    let result = DotNet.exec id command args
    Support.ensureSuccessExitCode result.ExitCode


let private clean _ =
    ["." ; "prelude" ; "damo-io-server" ; "somanyfeeds-server" ; "feeds-processing"; "feeds-processing-tests" ; "frontends"]
    |> List.collect (fun p -> [ Path.Combine (p, "bin") ; Path.Combine (p, "obj") ])
    |> List.map Directory.delete
    |> ignore


let private somanyfeedsServerIntegrationTests _ =
    Environment.setEnvironVar "PORT" "9090"
    Environment.setEnvironVar "CONTENT_ROOT" (Path.GetFullPath "somanyfeeds-server")
    Environment.setEnvironVar "DB_CONNECTION" "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_integration_tests"
    dotnet "run" "-p somanyfeeds-server-integration-tests" ()
    ()


let private setupCacheBustingLinks _ =
    let timestamp = DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()
    let timestampPath = "somanyfeeds-server/bin/Release/netcoreapp2.2/publish/Resources/templates/_assets_version.html.liquid"
    File.write false timestampPath [timestamp]
    ()


[<EntryPoint>]
let main args =
    use ctxt = fakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    Frontend.loadTasks ()
    Database.loadTasks ()

    Target.create "clean" clean
    Target.create "restore" <| dotnet "restore" ""
    Target.create "build" <| dotnet "build" ""
    Target.create "test" <| dotnet "test" "feeds-processing-tests"
    Target.create "integration-tests" <| somanyfeedsServerIntegrationTests

    Target.create "publish" (fun _ ->
        dotnet "publish" "damo-io-server -c Release" ()
        dotnet "publish" "somanyfeeds-server -c Release" ()
        setupCacheBustingLinks ()
    )


    "clean" |> dependsOn [ "frontend:clean" ]
    "build" |> dependsOn [ "frontend:build" ; "restore" ]

    "build" |> mustRunAfter "clean"

    "test" |> dependsOn [ "build" ]
    "integration-tests" |> dependsOn [ "build" ]
    "publish" |> dependsOn [ "test" ; "integration-tests" ; "build" ; "clean" ]

    Target.runOrDefault "publish"

    0
