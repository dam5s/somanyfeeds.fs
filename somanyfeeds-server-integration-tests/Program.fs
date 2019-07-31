module SoManyFeedsIntegrationTests

open IntegrationTests
open OpenQA.Selenium.Chrome
open System
open canopy
open canopy.runner.classic
open canopy.classic
open canopy.types
open configuration


let private homeDir : string =
    Environment.GetFolderPath Environment.SpecialFolder.UserProfile


let private env (name : string) : string =
    match Environment.GetEnvironmentVariable name with
    | null -> failwithf "Failed to load environment variable with name %s" name
    | value -> value


let private chromeOptions =
    let chromeOptions = new ChromeOptions ()
    chromeOptions.AddArguments ("--headless", "--disable-gpu", "--disable-ipv6")
    chromeOptions


[<EntryPoint>]
let main (_) =
    chromeDir <- env "CHROME_DRIVER_DIR"

    (chromeOptions, TimeSpan.FromSeconds 20.0)
    |> ChromeWithOptionsAndTimeSpan
    |> start

    Feeds.all ()
    FeedsProcessing.all ()

    run ()
    quit ()

    failedCount
