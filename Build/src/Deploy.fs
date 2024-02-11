module Deploy

open Fake.Core
open Support

let private runCmd cmd workingDir args =
    { Program = cmd; WorkingDir = workingDir; CommandLine = args; Args = [] }
    |> Process.shellExec
    |> Proc.ensureSuccessExitCode

let private herokuBinary =
    if Environment.isWindows
        then "heroku.cmd"
        else "heroku"

let private deploy project herokuApp _ =
    let publishDir = $"%s{project}/bin/Release/net8.0/linux-x64/publish"

    DotNet.release project ()

    File.write
        $"%s{publishDir}/Procfile"
        $"web: chmod 755 %s{project} && ./%s{project}"

    let buildOptions =
        if Environment.isMacOS
            then "--tar /opt/homebrew/bin/gtar"
            else ""

    runCmd
        herokuBinary
        publishDir
        $"builds:create -a %s{herokuApp} %s{buildOptions}"

let loadTasks _ =
    Target.create "deploy" (deploy "Damo.Io.Server" "damo-io")

    "deploy" |> Target.dependsOn [ "release" ]
