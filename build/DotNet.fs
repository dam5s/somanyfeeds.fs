module DotNet


open System.IO
open Fake.DotNet


let private dotnet command args =
    let result = DotNet.exec id command args
    Support.ensureSuccessExitCode result.ExitCode


let restore _ =
    dotnet "restore" ""

let build _ =
    dotnet "build" ""

let test project =
    dotnet "test" project

let run project _ =
    dotnet "run" (sprintf "-p %s" project)

let release project _ =
    dotnet "publish" (sprintf "%s -c Release -r linux-x64" project)


let solutionProjects =
    File.ReadAllLines("somanyfeeds.sln")
    |> Array.filter (fun line -> line.StartsWith("Project"))
    |> Array.map (fun line -> Array.get (line.Split "=") 1 )
    |> Array.map (fun line -> Array.get (line.Split ",") 0 )
    |> Array.map (fun line -> line.Trim().Replace("\"", "") )
    |> Array.toList

let projectSubFolder name projectName =
    DirectoryInfo(sprintf "%s/%s" projectName name)

let clean _ =
    let binFolders = solutionProjects |> List.map (projectSubFolder "bin")
    let objFolders = solutionProjects |> List.map (projectSubFolder "obj")

    binFolders @ objFolders
    |> List.filter (fun dir -> dir.Exists)
    |> List.map (fun dir -> dir.Delete true)
    |> ignore
