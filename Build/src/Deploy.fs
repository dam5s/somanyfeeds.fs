module Deploy

open Fake.Core
open Support

let private runCmd cmd workingDir args =
    { Program = cmd; WorkingDir = workingDir; CommandLine = args; Args = [] }
    |> Process.shellExec
    |> ensureSuccessExitCode

let private herokuBinary =
    if Environment.isWindows
        then "heroku.cmd"
        else "heroku"

let private deploy project herokuApp _ =
    let publishDir = $"%s{project}/bin/Release/net6.0/linux-x64/publish"

    DotNet.release project ()

    writeToFile
        $"%s{publishDir}/Procfile"
        $"web: chmod 755 %s{project} && ./%s{project}"

    runCmd
        herokuBinary
        publishDir
        $"builds:create -a %s{herokuApp}"

let loadTasks _ =
    Target.create "deploy:damo-io" (deploy "Damo.Io.Server" "damo-io")

    "deploy:damo-io" |> dependsOn [ "release" ]
