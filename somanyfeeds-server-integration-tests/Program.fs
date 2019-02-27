﻿module SoManyFeedsIntegrationTests

open IntegrationTests
open OpenQA.Selenium.Chrome
open SoManyFeedsServer
open Suave
open Suave.Logging
open System
open System.Threading
open canopy
open canopy.runner.classic
open canopy.classic
open canopy.types
open configuration


let private homeDir : string =
    Environment.GetFolderPath Environment.SpecialFolder.UserProfile

let mutable private tokenSource : CancellationTokenSource option =
    None

let private chromeOptions =
    let chromeOptions = new ChromeOptions ()
    chromeOptions.AddArguments ("--headless", "--disable-gpu", "--disable-ipv6")
    chromeOptions


once (fun () ->
    LoggingConfig.configure ()

    let logger = Log.create "suave"
    let config = SoManyFeedsServer.WebConfig.create logger
    let webPart = SoManyFeedsServer.WebApp.webPart

    let listening, server = startWebServerAsync config webPart
    let tokenSource = new CancellationTokenSource ()

    Async.Start (server, tokenSource.Token)

    listening
    |> Async.RunSynchronously
    |> ignore
)


lastly (fun () ->
    tokenSource
    |> Option.map (fun src -> src.Cancel ())
    |> ignore
)


[<EntryPoint>]
let main (_) =
    chromeDir <- sprintf "%s/dev/chromedriver-2.44" homeDir
    start <| ChromeWithOptionsAndTimeSpan (chromeOptions, TimeSpan.FromSeconds 20.0)

    Feeds.all ()

    run ()
    quit ()

    failedCount
