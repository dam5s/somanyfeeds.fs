module Frontend

open Fake.Core
open Fake.IO
open SharpScss
open Support
open System.IO

let private generateCss filePath =
    let scssOptions = ScssOptions(OutputStyle = ScssOutputStyle.Compressed)
    let result = Scss.ConvertFileToCss(filePath, scssOptions)
    result.Css

let private runCmd cmd workingDir args =
    { Program = cmd; WorkingDir = workingDir; CommandLine = args; Args = [] }
    |> Process.shellExec
    |> ensureSuccessExitCode

let private clean _ =
    File.delete "damo-io-server/Resources/public/damo-io.js"
    File.delete "damo-io-server/Resources/public/damo-io.css"
    File.delete "somanyfeeds-server/WebRoot/somanyfeeds.css"
    File.delete "somanyfeeds-server/WebRoot/somanyfeeds.js"
    Directory.delete "damo-io-frontend/elm-stuff/0.19.1"

let private buildScss _ =
    generateCss "shared-frontend/Scss/damo-io.scss" |> writeToFile "damo-io-server/Resources/public/damo-io.css"
    generateCss "shared-frontend/Scss/somanyfeeds.scss" |> writeToFile "somanyfeeds-server/WebRoot/somanyfeeds.css"

let private buildDamoIoFrontend _ =
    runCmd "elm" "damo-io-frontend" "make --optimize --output ../../damo-io-server/Resources/public/damo-io.js DamoIO/App.elm"

let private buildSoManyFeedsFrontend _ =
    runCmd "yarn.cmd" "somanyfeeds-frontend" "install -s"
    runCmd "yarn.cmd" "somanyfeeds-frontend" "run build"

let private copyFonts _ =
    Shell.copyDir "damo-io-server/Resources/public/fonts" "frontends/Fonts" (fun _ -> true)
    Shell.copyDir "somanyfeeds-server/WebRoot/fonts" "frontends/Fonts" (fun _ -> true)

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
