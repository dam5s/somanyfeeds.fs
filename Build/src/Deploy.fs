module Deploy

open System
open Fake.Core
open Support

let private runCmd cmd workingDir args =
    { Program = cmd; WorkingDir = workingDir; CommandLine = args; Args = [] }
    |> Process.shellExec
    |> ensureSuccessExitCode

let private runningOnWindows =
    match Environment.OSVersion.Platform with
    | PlatformID.Win32NT
    | PlatformID.Win32S
    | PlatformID.Win32Windows
    | PlatformID.WinCE -> true
    | _ -> false

let private herokuBinary =
    if runningOnWindows
        then "heroku.cmd"
        else "heroku"

let private deploy project _ =
    let publishDir = $"%s{project}/bin/Release/net5.0/linux-x64/publish"
    let herokuApp = project.Replace("-server", "")

    DotNet.release project ()

    writeToFile
        $"%s{publishDir}/Procfile"
        $"web: chmod 755 %s{project} && ./%s{project}"

    runCmd
        herokuBinary
        publishDir
        $"builds:create -a %s{herokuApp}"

let loadTasks _ =
    Target.create "deploy:somanyfeeds" (deploy "SoManyFeeds.Server")
    Target.create "deploy:damo-io" (deploy "Damo.Io.Server")

    "deploy:somanyfeeds" |> dependsOn [ "release" ]
    "deploy:damo-io" |> dependsOn [ "release" ]
