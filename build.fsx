#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet
nuget Fake.DotNet.Cli
//"
#load "./.fake/build.fsx/intellisense.fsx"


open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet


exception BuildException

let dotnet (command : string) (args : string) = (fun _ ->
    let result = DotNet.exec id command args

    match result.ExitCode with
    | 0 -> ()
    | _ -> raise BuildException
)


Target.create "clean" (dotnet "clean" "")

Target.create "restore" (dotnet "restore" "")

Target.create "buildFrontend" (dotnet "run" "-p frontend/frontend.fsproj -- frontend")

Target.create "build" (dotnet "build" "")

Target.create "test" (dotnet "test" "server-tests")

Target.create "publish" (dotnet "publish" "server -c Release")


"clean"
    ==> "restore"
    ==> "buildFrontend"
    ==> "build"
    ==> "test"
    ==> "publish"


Target.runOrDefault "publish"
