[<RequireQualifiedAccess>]
module DotNet

open System.IO

let private dotnet command =
    Proc.exec $"dotnet %s{command}"

let build _ =
    dotnet "build"

let test _ =
    dotnet "test"

let release project _ =
    dotnet $"publish %s{project} -c Release -r linux-x64 --self-contained"

type Project(name: string) =
    let subDir dir =
        DirectoryInfo $"{name}/{dir}"

    member _.buildFolders =
        [ "bin"; "obj" ] |> List.map subDir

[<RequireQualifiedAccess>]
type Solution(fileName: string) =
    member self.projects =
        File.ReadAllLines(fileName)
        |> Array.filter (fun line -> line.StartsWith("Project"))
        |> Array.map (fun line -> Array.get (line.Split "=") 1 )
        |> Array.map (fun line -> Array.get (line.Split ",") 0 )
        |> Array.map (fun line -> line.Trim().Replace("\"", "") )
        |> Array.map Project
        |> Array.toList

    member self.buildFolders =
        self.projects |> List.collect _.buildFolders

let clean _ =
    let solution = Solution("SoManyFeeds.sln")

    solution.buildFolders
    |> List.filter (fun dir -> dir.Exists)
    |> List.filter (fun dir -> dir.Parent.Name <> "Build")
    |> List.map (fun dir -> dir.Delete true)
    |> ignore
