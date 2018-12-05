open SharpScss
open System
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

    use writer = File.CreateText(filePath)
    writer.WriteLine(content)


let private dotnet (command : string) (args : string) _ =
    let result = DotNet.exec id command args
    ensureSuccessExitCode result.ExitCode


let private generateCss (filePath : string) : string =
    let scssOptions = new ScssOptions (OutputStyle = ScssOutputStyle.Compressed)
    let result = Scss.ConvertFileToCss(filePath, scssOptions)
    result.Css


let private cleanScss _ =
    File.delete "damo-io-server/resources/public/app.css"


let private cleanElm _ =
    Directory.delete "frontends/src/elm/elm-stuff/0.19.0"


let private clean _ =
    cleanElm ()
    cleanScss ()

    ["." ; "damo-io-server" ; "damo-io-server-tests" ; "frontends"]
        |> List.map (fun p ->
            Path.Combine(p, "bin") |> Directory.delete
            Path.Combine(p, "obj") |> Directory.delete
        )
        |> ignore


let private buildScss _ =
    generateCss "frontends/src/scss/app.scss" |> writeToFile "damo-io-server/resources/public/app.css"


let private buildElm _ =
    let args =
        { Program = "elm"
          WorkingDir = "frontends/src/elm"
          CommandLine = "make --optimize --output ../../../damo-io-server/resources/public/app.js DamoIO/App.elm"
          Args = []
        }
    Process.shellExec args |> ensureSuccessExitCode


let private copyAssets _ =
    Shell.copyDir "damo-io-server/resources/public" "frontends/src" (fun f ->
        not (f.Contains "elm")
            && not (f.Contains "scss")
            && not (f.EndsWith ".fs")
    )


let private buildFakeExecutionContext (args : string list) =
    let fakeArgs =
        if (List.isEmpty args) then
            []
        else
            [ "--target"
              (List.head args)
            ]

    FakeExecutionContext.Create false "Program.fs" fakeArgs

let dependsOn (tasks : string list) (task : string) = task <== tasks
let mustRunAfter (otherTask : string) (task : string) = task <=? otherTask |> ignore

[<EntryPoint>]
let main (args : string []) =
    use ctxt = buildFakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    Target.create "clean" clean
    Target.create "buildScss" buildScss
    Target.create "buildElm" buildElm
    Target.create "copyAssets" copyAssets
    Target.create "restore" <| dotnet "restore" ""
    Target.create "build" <| dotnet "build" ""
    Target.create "test" <| dotnet "test" "damo-io-server-tests"
    Target.create "publish" <| dotnet "publish" "damo-io-server -c Release"


    "copyAssets" |> dependsOn [ "buildElm" ; "buildScss" ]

    "build" |> dependsOn [ "copyAssets" ; "restore" ]

    "build" |> mustRunAfter "clean"
    "buildElm" |> mustRunAfter "clean"
    "buildScss" |> mustRunAfter "clean"

    "test" |> dependsOn [ "build" ]

    "publish" |> dependsOn [ "test" ; "build" ; "clean" ]

    Target.runOrDefault "publish"

    0
