#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet
nuget Fake.DotNet.Cli
//"
#load "./.fake/build.fsx/intellisense.fsx"


open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open System


exception BuildException


let private ensureSuccessExitCode (exitCode : int) =
    match exitCode with
    | 0 -> ()
    | _ -> raise BuildException


let private dotnet (command : string) (args : string) = (fun _ ->
    let result = DotNet.exec id command args
    ensureSuccessExitCode result.ExitCode
)

let private subProjects = ["." ; "server" ; "server-tests" ; "frontend"]


Target.create "clean" (fun _ ->
    List.map (fun (path : string) ->
        Directory.delete (String.Format("{0}/bin", path)) |> ignore
        Directory.delete (String.Format("{0}/obj", path)) |> ignore
    ) subProjects |> ignore

    Directory.delete "server/WebRoot"
    Directory.delete "frontend/src/elm/elm-stuff/build-artifacts/0.18.0/dam5s"

    ()
)

Target.create "restore" (dotnet "restore" "")

Target.create "compileScss" (dotnet "run" "-p frontend/frontend.fsproj -- frontend")

Target.create "compileElm" (fun _ ->
    let args =
        { Program = "elm-make"
          WorkingDir = "frontend/src/elm"
          CommandLine = "SoManyFeeds/App.elm --output ../../../server/WebRoot/app.js --yes"
          Args = []
        }
    Process.shellExec args |> ensureSuccessExitCode
)

Target.create "copyAssets" (fun _ ->
    Shell.copyDir "server/WebRoot" "frontend/src" (fun f ->
        not (f.Contains "elm")
            && not (f.Contains "scss")
            && not (f.EndsWith ".fs")
    )
    ()
)

Target.create "build" (dotnet "build" "")

Target.create "test" (dotnet "test" "server-tests")

Target.create "publish" (dotnet "publish" "server -c Release")


"clean"
    ==> "restore"
    ==> "compileScss"
    ==> "compileElm"
    ==> "copyAssets"
    ==> "build"
    ==> "test"
    ==> "publish"


Target.runOrDefault "publish"
