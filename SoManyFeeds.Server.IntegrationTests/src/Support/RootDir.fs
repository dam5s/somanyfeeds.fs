[<AutoOpen>]
module RootDir

open System.IO

let projectDir projectName =
    let parent dir =
        Directory.GetParent(dir).FullName

    let rec findRootDir dir =
        if File.Exists(Path.Combine(dir, "SoManyFeeds.sln"))
            then dir
            else findRootDir (parent dir)

    let rootDir =
        findRootDir (Directory.GetCurrentDirectory())

    Path.Combine(rootDir, projectName)
