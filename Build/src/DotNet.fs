[<RequireQualifiedAccess>]
module DotNet

open System.IO

let private dotnet command = Proc.exec $"dotnet %s{command}"

let build _ = dotnet "build"

let test _ = dotnet "test"

let release project _ =
    dotnet $"publish %s{project} -c Release -r linux-x64 --self-contained"

type Project(name: string) =

    let subDirPath dir = $"{name}/{dir}"
    let subDir = subDirPath >> DirectoryInfo

    member _.buildDirs = [ "bin"; "obj" ] |> List.map subDir

    member _.srcDirPath = subDirPath "src"

[<RequireQualifiedAccess>]
type Solution(fileName: string) =
    member self.projects =
        File.ReadAllLines(fileName)
        |> Array.filter (fun line -> line.StartsWith("Project"))
        |> Array.map (fun line -> Array.get (line.Split "=") 1)
        |> Array.map (fun line -> Array.get (line.Split ",") 0)
        |> Array.map (fun line -> line.Trim().Replace("\"", ""))
        |> Array.map Project
        |> Array.toList

    member self.buildDirs = self.projects |> List.collect _.buildDirs

    member self.srcDirPaths = self.projects |> List.map _.srcDirPath

let clean _ =
    let sln = Solution("SoManyFeeds.sln")

    sln.buildDirs
    |> List.filter (fun dir -> dir.Exists)
    |> List.filter (fun dir -> dir.Parent.Name <> "Build")
    |> List.map (fun dir -> dir.Delete true)
    |> ignore
