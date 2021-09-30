module Frontend

open Fake.Core
open Fake.IO
open SharpScss
open Support

let private generateCss filePath =
    let scssOptions = ScssOptions(OutputStyle = ScssOutputStyle.Compressed)
    let result = Scss.ConvertFileToCss(filePath, scssOptions)
    result.Css

let private runCmd cmd workingDir args =
    { Program = cmd; WorkingDir = workingDir; CommandLine = args; Args = [] }
    |> Process.shellExec
    |> ensureSuccessExitCode

let private clean _ =
    File.delete "Damo.Io.Server/resources/WebRoot/damo-io.js"
    File.delete "Damo.Io.Server/resources/WebRoot/damo-io.css"
    File.delete "SoManyFeeds.Server/WebRoot/somanyfeeds.css"
    File.delete "SoManyFeeds.Server/WebRoot/somanyfeeds.js"

let private buildScss _ =
    generateCss "Frontend.Shared/Scss/damo-io.scss" |> writeToFile "Damo.Io.Server/WebRoot/damo-io.css"
    generateCss "Frontend.Shared/Scss/somanyfeeds.scss" |> writeToFile "SoManyFeeds.Server/WebRoot/somanyfeeds.css"

let private yarn =
    if Environment.isWindows
        then "yarn.cmd"
        else "yarn"

let private buildDamoIoFrontend _ =
    runCmd yarn "Damo.Io.Frontend" "install -s"
    runCmd yarn "Damo.Io.Frontend" "run build"

let private buildSoManyFeedsFrontend _ =
    runCmd yarn "SoManyFeeds.Frontend" "install -s"
    runCmd yarn "SoManyFeeds.Frontend" "run build"

let private copyFonts _ =
    Shell.copyDir "Damo.Io.Server/WebRoot/fonts" "Frontend.Shared/Fonts" (fun _ -> true)
    Shell.copyDir "SoManyFeeds.Server/WebRoot/fonts" "Frontend.Shared/Fonts" (fun _ -> true)

let loadTasks _ =
    Target.create "frontend:clean" clean
    Target.create "frontend:scss" buildScss
    Target.create "frontend:damo-io" buildDamoIoFrontend
    Target.create "frontend:somanyfeeds" buildSoManyFeedsFrontend
    Target.create "frontend:fonts" copyFonts
    Target.create "frontend:build" ignore

    "frontend:build" |> dependsOn [ "frontend:scss"; "frontend:damo-io"; "frontend:somanyfeeds"; "frontend:fonts" ]
    "frontend:scss" |> mustRunAfter "frontend:clean"
    "frontend:damo-io" |> mustRunAfter "frontend:clean"
    "frontend:somanyfeeds" |> mustRunAfter "frontend:clean"
    "frontend:fonts" |> mustRunAfter "frontend:clean"
