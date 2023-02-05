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

let private buildScss _ =
    generateCss "Frontend.Shared/Scss/damo-io.scss" |> writeToFile "Damo.Io.Server/WebRoot/damo-io.css"

let private npm =
    if Environment.isWindows
        then "npm.cmd"
        else "npm"

let private buildDamoIoFrontend _ =
    runCmd npm "Damo.Io.Frontend" "install -s"
    runCmd npm "Damo.Io.Frontend" "run build"

let private copyFonts _ =
    Shell.copyDir "Damo.Io.Server/WebRoot/fonts" "Frontend.Shared/Fonts" (fun _ -> true)

let loadTasks _ =
    Target.create "frontend:clean" clean
    Target.create "frontend:scss" buildScss
    Target.create "frontend:damo-io" buildDamoIoFrontend
    Target.create "frontend:fonts" copyFonts
    Target.create "frontend:build" ignore

    "frontend:build" |> dependsOn [ "frontend:scss"; "frontend:damo-io"; "frontend:fonts" ]
    "frontend:scss" |> mustRunAfter "frontend:clean"
    "frontend:damo-io" |> mustRunAfter "frontend:clean"
    "frontend:fonts" |> mustRunAfter "frontend:clean"
