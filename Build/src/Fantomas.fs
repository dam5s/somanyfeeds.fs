[<RequireQualifiedAccess>]
module Fantomas

let private srcDirs () =
    let sln = DotNet.Solution("SoManyFeeds.sln")
    let srcDirs = sln.projects |> List.map (fun p -> p.srcDirPath)
    String.concat " " srcDirs

let format _ =
    Proc.exec $"dotnet fantomas %s{srcDirs ()}"

let check _ =
    Proc.exec $"dotnet fantomas --check %s{srcDirs ()}"
