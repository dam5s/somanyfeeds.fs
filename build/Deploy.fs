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
    let publishDir = sprintf "%s/bin/Release/net5.0/linux-x64/publish" project
    let herokuApp = project.Replace("-server", "")

    DotNet.release project ()

    writeToFile
        (sprintf "%s/Procfile" publishDir)
        (sprintf "web: chmod 755 %s && ./%s" project project)

    runCmd
        herokuBinary
        publishDir
        (sprintf "builds:create -a %s" herokuApp)

let loadTasks _ =
    Target.create "deploy:somanyfeeds" (deploy "somanyfeeds-server")
    Target.create "deploy:damo-io" (deploy "damo-io-server")

    "deploy:somanyfeeds" |> dependsOn [ "release" ]
    "deploy:damo-io" |> dependsOn [ "release" ]
