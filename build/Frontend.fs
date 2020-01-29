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
    File.delete "somanyfeeds-server/Resources/public/somanyfeeds.css"
    File.delete "somanyfeeds-server/Resources/public/somanyfeeds.js"
    File.delete "somanyfeeds-server/Resources/public/somanyfeeds-fable.js"
    Directory.delete "frontends/Elm/elm-stuff/0.19.0"


let private buildScss _ =
    generateCss "frontends/Scss/damo-io.scss" |> writeToFile "damo-io-server/Resources/public/damo-io.css"
    generateCss "frontends/Scss/somanyfeeds.scss" |> writeToFile "somanyfeeds-server/Resources/public/somanyfeeds.css"


let private buildElm _ =
    let somanyfeedsApps =
        Directory.GetFiles("frontends/Elm/SoManyFeeds/Applications", "*.elm")
        |> Array.map (String.replaceFirst "frontends/Elm/" "")
        |> String.concat " "

    runCmd "elm" "frontends/Elm" "make --optimize --output ../../damo-io-server/Resources/public/damo-io.js DamoIO/App.elm"
    runCmd "elm" "frontends/Elm" (sprintf "make --optimize --output ../../somanyfeeds-server/Resources/public/somanyfeeds.js %s" somanyfeedsApps)


let private buildFable _ =
    runCmd "yarn.cmd" "fable-frontend" "install -s"
    runCmd "yarn.cmd" "fable-frontend" "run build"

let private copyFonts _ =
    Shell.copyDir "damo-io-server/Resources/public/fonts" "frontends/Fonts" (fun _ -> true)
    Shell.copyDir "somanyfeeds-server/Resources/public/fonts" "frontends/Fonts" (fun _ -> true)


let loadTasks _ =
    Target.create "frontend:clean" clean
    Target.create "frontend:scss" buildScss
    Target.create "frontend:elm" buildElm
    Target.create "frontend:fable" buildFable
    Target.create "frontend:fonts" copyFonts
    Target.create "frontend:build" ignore

    "frontend:build" |> dependsOn [ "frontend:scss"; "frontend:elm"; "frontend:fable"; "frontend:fonts" ]
    "frontend:scss" |> mustRunAfter "frontend:clean"
    "frontend:elm" |> mustRunAfter "frontend:clean"
    "frontend:fable" |> mustRunAfter "frontend:clean"
    "frontend:fonts" |> mustRunAfter "frontend:clean"
