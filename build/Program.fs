open System
open SharpScss
open System.IO
open Fake.Core
open Fake.Core.Context
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.Runtime


exception BuildException


let private ensureSuccessExitCode (exitCode : int) =
    match exitCode with
    | 0 -> ()
    | _ -> raise BuildException


let private writeToFile (filePath : string) (content : string) =
    Directory.GetParent(filePath).Create()

    use writer = File.CreateText filePath
    writer.WriteLine content


let private dotnet (command : string) (args : string) _ =
    let result = DotNet.exec id command args
    ensureSuccessExitCode result.ExitCode


let private generateCss (filePath : string) : string =
    let scssOptions = new ScssOptions (OutputStyle = ScssOutputStyle.Compressed)
    let result = Scss.ConvertFileToCss (filePath, scssOptions)
    result.Css


let private cleanScss _ =
    File.delete "damo-io-server/Resources/public/damo-io.css"
    File.delete "somanyfeeds-server/Resources/public/somanyfeeds.css"


let private cleanElm _ =
    Directory.delete "frontends/Elm/elm-stuff/0.19.0"


let private clean _ =
    cleanElm ()
    cleanScss ()

    ["." ; "prelude" ; "damo-io-server" ; "somanyfeeds-server" ; "feeds-processing"; "feeds-processing-tests" ; "frontends"]
    |> List.map (fun p ->
        Path.Combine (p, "bin") |> Directory.delete
        Path.Combine (p, "obj") |> Directory.delete
    )
    |> ignore


let private buildScss _ =
    generateCss "frontends/Scss/damo-io.scss" |> writeToFile "damo-io-server/Resources/public/damo-io.css"
    generateCss "frontends/Scss/somanyfeeds.scss" |> writeToFile "somanyfeeds-server/Resources/public/somanyfeeds.css"


let private runElm elmArgs =
    let args =
        { Program = "elm"
          WorkingDir = "frontends/Elm"
          CommandLine = elmArgs
          Args = []
        }
    Process.shellExec args |> ensureSuccessExitCode


let private buildElm _ =
    runElm "make --optimize --output ../../damo-io-server/Resources/public/damo-io.js DamoIO/App.elm"
    runElm "make --optimize --output ../../somanyfeeds-server/Resources/public/somanyfeeds.js SoManyFeeds/Read.elm SoManyFeeds/Manage.elm"


let private copyFonts _ =
    Shell.copyDir "damo-io-server/Resources/public/fonts" "frontends/Fonts" (fun f -> true)
    Shell.copyDir "somanyfeeds-server/Resources/public/fonts" "frontends/Fonts" (fun f -> true)


let private buildFakeExecutionContext (args : string list) =
    let fakeArgs =
        if List.isEmpty args then
            []
        else
            [ "--target"
              List.head args
            ]

    FakeExecutionContext.Create false "Program.fs" fakeArgs


let private somanyfeedsServerIntegrationTests _ =
    Environment.setEnvironVar "PORT" "9090"
    Environment.setEnvironVar "CONTENT_ROOT" (Path.GetFullPath "somanyfeeds-server")
    Environment.setEnvironVar "DB_CONNECTION" "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_integration_tests"
    dotnet "run" "-p somanyfeeds-server-integration-tests" ()
    ()


let private setupCacheBustingLinks _ =
    let timestamp = DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()
    let timestampPath = "somanyfeeds-server/bin/Release/netcoreapp2.1/publish/Resources/templates/_assets_version.html.liquid"
    File.write false timestampPath [timestamp]
    ()


let dependsOn (tasks : string list) (task : string) = task <== tasks
let mustRunAfter (otherTask : string) (task : string) = task <=? otherTask |> ignore


[<EntryPoint>]
let main (args : string []) =
    use ctxt = buildFakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    Target.create "clean" clean
    Target.create "buildScss" buildScss
    Target.create "buildElm" buildElm
    Target.create "copyFonts" copyFonts
    Target.create "restore" <| dotnet "restore" ""
    Target.create "build" <| dotnet "build" ""

    Target.create "test" <| dotnet "test" "feeds-processing-tests"
    Target.create "integration-tests" <| somanyfeedsServerIntegrationTests

    Target.create "publish" (fun _ ->
        dotnet "publish" "damo-io-server -c Release" ()
        dotnet "publish" "somanyfeeds-server -c Release" ()
        setupCacheBustingLinks ()
    )


    "copyFonts" |> dependsOn [ "buildElm" ; "buildScss" ]
    "build" |> dependsOn [ "copyFonts" ; "restore" ]

    "build" |> mustRunAfter "clean"
    "buildElm" |> mustRunAfter "clean"
    "buildScss" |> mustRunAfter "clean"

    "test" |> dependsOn [ "build" ]
    "integration-tests" |> dependsOn [ "build" ]
    "publish" |> dependsOn [ "test" ; "integration-tests" ; "build" ; "clean" ]

    Target.runOrDefault "publish"

    0
