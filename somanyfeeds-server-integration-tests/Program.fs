module SoManyFeedsIntegrationTests

open IntegrationTests
open OpenQA.Selenium.Chrome
open System
open canopy
open canopy.classic
open canopy.runner.classic
open canopy.types
open configuration

open SoManyFeeds


let private chromeOptions =
    let chromeOptions = ChromeOptions()
    chromeOptions.AddArguments("--headless", "--disable-gpu", "--disable-ipv6")
    chromeOptions

[<EntryPoint>]
let main (_) =
    chromeDir <- Env.requireVar "CHROME_DRIVER_DIR"

    (chromeOptions, TimeSpan.FromSeconds 20.0)
    |> ChromeWithOptionsAndTimeSpan
    |> start

    Feeds.all()
    FeedsProcessing.all()

    run()
    quit()

    failedCount
