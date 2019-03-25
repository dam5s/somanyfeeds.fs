module DotNet


open Fake.DotNet


let private dotnet (command : string) (args : string) =
    let result = DotNet.exec id command args
    Support.ensureSuccessExitCode result.ExitCode


let restore _ =
    dotnet "restore" ""

let build _ =
    dotnet "build" ""

let test _ =
    dotnet "test" ""

let run project _ =
    dotnet "run" (sprintf "-p %s" project)

let release project _ =
    dotnet "publish" (sprintf "%s -c Release" project)
