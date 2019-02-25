module Frontend

open Fake.Core
open Fake.IO
open SharpScss
open Support


let private generateCss (filePath : string) : string =
    let scssOptions = new ScssOptions (OutputStyle = ScssOutputStyle.Compressed)
    let result = Scss.ConvertFileToCss (filePath, scssOptions)
    result.Css


let private runElm elmArgs =
    { Program = "elm"
      WorkingDir = "frontends/Elm"
      CommandLine = elmArgs
      Args = []
    }
    |> Process.shellExec
    |> Support.ensureSuccessExitCode


let private clean _ =
    File.delete "damo-io-server/Resources/public/damo-io.js"
    File.delete "damo-io-server/Resources/public/damo-io.css"
    File.delete "somanyfeeds-server/Resources/public/somanyfeeds.css"
    File.delete "somanyfeeds-server/Resources/public/somanyfeeds.js"
    Directory.delete "frontends/Elm/elm-stuff/0.19.0"


let private buildScss _ =
    generateCss "frontends/Scss/damo-io.scss" |> Support.writeToFile "damo-io-server/Resources/public/damo-io.css"
    generateCss "frontends/Scss/somanyfeeds.scss" |> Support.writeToFile "somanyfeeds-server/Resources/public/somanyfeeds.css"


let private buildElm _ =
    runElm "make --optimize --output ../../damo-io-server/Resources/public/damo-io.js DamoIO/App.elm"
    runElm "make --optimize --output ../../somanyfeeds-server/Resources/public/somanyfeeds.js SoManyFeeds/Read.elm SoManyFeeds/Manage.elm SoManyFeeds/Register.elm"


let private copyFonts _ =
    Shell.copyDir "damo-io-server/Resources/public/fonts" "frontends/Fonts" (fun f -> true)
    Shell.copyDir "somanyfeeds-server/Resources/public/fonts" "frontends/Fonts" (fun f -> true)


let loadTasks _ =
    Target.create "frontend:clean" clean
    Target.create "frontend:scss" buildScss
    Target.create "frontend:elm" buildElm
    Target.create "frontend:fonts" copyFonts
    Target.create "frontend:build" (fun _ -> ())

    "frontend:build" |> dependsOn [ "frontend:scss" ; "frontend:elm" ; "frontend:fonts" ]
    "frontend:scss" |> mustRunAfter "frontend:clean"
    "frontend:elm" |> mustRunAfter "frontend:clean"
    "frontend:fonts" |> mustRunAfter "frontend:clean"
