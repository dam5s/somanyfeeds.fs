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


let private dotnet (command : string) (args : string) =
    let result = DotNet.exec id command args
    ensureSuccessExitCode result.ExitCode


let private generateCss (filePath : string) : string =
    let scssOptions = new ScssOptions (OutputStyle = ScssOutputStyle.Compressed)
    let result = Scss.ConvertFileToCss(filePath, scssOptions)
    result.Css


let private writeToFile (filePath : string) (content : string) =
    Directory.GetParent(filePath).Create()

    use writer = File.CreateText(filePath)
    writer.WriteLine(content)


let private cleanScss (p : TargetParameter) =
    File.delete "server/WebRoot/app.css"


let private cleanElm (p : TargetParameter) =
    Directory.delete "frontend/src/elm/elm-stuff/build-artifacts/0.18.0/dam5s"


let private clean (p : TargetParameter) =
    cleanElm p
    cleanScss p

    ["." ; "server" ; "server-tests" ; "frontend"]
        |> List.map (fun p ->
            Path.Combine(p, "bin") |> Directory.delete
            Path.Combine(p, "obj") |> Directory.delete
        )
        |> ignore


let private buildScss (p : TargetParameter) =
    generateCss "frontend/src/scss/app.scss" |> writeToFile "server/WebRoot/app.css"


let private buildElm (p : TargetParameter) =
    let args =
        { Program = "elm-make"
          WorkingDir = "frontend/src/elm"
          CommandLine = "SoManyFeeds/App.elm --output ../../../server/WebRoot/app.js --yes"
          Args = []
        }
    Process.shellExec args |> ensureSuccessExitCode


let private copyAssets (p : TargetParameter) =
    Shell.copyDir "server/WebRoot" "frontend/src" (fun f ->
        not (f.Contains "elm")
            && not (f.Contains "scss")
            && not (f.EndsWith ".fs")
    )


let private restore (p : TargetParameter) = dotnet "restore" ""

let private build (p : TargetParameter) = dotnet "build" ""

let private test (p : TargetParameter) = dotnet "test" "server-tests"

let private publish (p : TargetParameter) = dotnet "publish" "server -c Release"


let private buildFakeExecutionContext (args : string list) =
    let fakeArgs =
        if (List.isEmpty args) then
            []
        else
            [ "--target"
              (List.head args)
            ]

    FakeExecutionContext.Create false "Program.fs" fakeArgs


[<EntryPoint>]
let main (args : string []) =
    use ctxt = buildFakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    Target.create "clean" clean
    Target.create "buildScss" buildScss
    Target.create "buildElm" buildElm
    Target.create "copyAssets" copyAssets
    Target.create "restore" restore
    Target.create "build" build
    Target.create "test" test
    Target.create "publish" publish


    "buildElm" ==> "copyAssets" |> ignore
    "buildScss" ==> "copyAssets" |> ignore

    "copyAssets" ==> "build" |> ignore
    "restore" ==> "build" |> ignore

    "build" ==> "test" ==> "publish" |> ignore

    Target.runOrDefault "publish"

    0
