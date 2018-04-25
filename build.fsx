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



Target.create "clean" (dotnet "clean" "")

Target.create "restore" (dotnet "restore" "")

Target.create "build" (dotnet "build" "")

Target.create "test" (dotnet "test" "server-tests")



"clean"
    ==> "restore"
    ==> "build"
    ==> "test"

Target.runOrDefault "test"
