module SoManyFeedsIntegrationTests

open IntegrationTests
open OpenQA.Selenium.Chrome
open System
open canopy
open canopy.classic
open canopy.runner.classic
open canopy.types
open configuration


let private homeDir: string =
    Environment.GetFolderPath Environment.SpecialFolder.UserProfile


let private chromeOptions =
    let chromeOptions = new ChromeOptions()
    chromeOptions.AddArguments("--headless", "--disable-gpu", "--disable-ipv6")
    chromeOptions


[<EntryPoint>]
let main (_) =
    chromeDir <- Env.varRequired "CHROME_DRIVER_DIR"

    (chromeOptions, TimeSpan.FromSeconds 20.0)
    |> ChromeWithOptionsAndTimeSpan
    |> start

    Feeds.all()
    FeedsProcessing.all()

    run()
    quit()

    failedCount
