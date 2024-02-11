module DotNet

open System.IO
open Fake.Core


let private runCmd cmd args =
    { Program = cmd; WorkingDir = "."; CommandLine = args; Args = [] }
    |> Process.shellExec
    |> Proc.ensureSuccessExitCode

let private dotnet command args =
    runCmd "dotnet" $"%s{command} %s{args}"

let restore _ =
    dotnet "restore" ""

let build _ =
    dotnet "build" ""

let test project =
    dotnet "test" project

let run project _ =
    dotnet "run" $"-p %s{project}"

let release project _ =
    dotnet "publish" $"%s{project} -c Release -r linux-x64 --self-contained"

let solutionProjects =
    File.ReadAllLines("SoManyFeeds.sln")
    |> Array.filter (fun line -> line.StartsWith("Project"))
    |> Array.map (fun line -> Array.get (line.Split "=") 1 )
    |> Array.map (fun line -> Array.get (line.Split ",") 0 )
    |> Array.map (fun line -> line.Trim().Replace("\"", "") )
    |> Array.toList

let projectSubFolder name projectName =
    DirectoryInfo $"%s{projectName}/%s{name}"

let clean _ =
    let binFolders = solutionProjects |> List.map (projectSubFolder "bin")
    let objFolders = solutionProjects |> List.map (projectSubFolder "obj")

    binFolders @ objFolders
    |> List.filter (fun dir -> dir.Exists)
    |> List.filter (fun dir -> dir.Parent.Name <> "Build")
    |> List.map (fun dir -> dir.Delete true)
    |> ignore
