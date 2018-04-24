#r "paket:
nuget Fake.Core.Target
nuget Fake.DotNet
nuget Fake.DotNet.Cli
nuget NSass.Core
nuget NSass.Optimization
//"

#load "./.fake/build.fsx/intellisense.fsx"


open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet


exception BuildError

let dotnet (command : string) (args : string) = (fun _ ->
    let result = DotNet.exec id command args

    match result.ExitCode with
    | 0 -> ()
    | _ -> raise BuildError
)



Target.create "Clean" (dotnet "clean" "")

Target.create "Restore" (dotnet "restore" "")

Target.create "Build" (dotnet "build" "")

Target.create "Test" (dotnet "test" "server-tests")



"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Test"

Target.runOrDefault "Test"
