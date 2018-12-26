module SoManyFeedsIntegrationTests

open IntegrationTests
open System
open canopy
open canopy.runner.classic
open canopy.classic
open configuration


let homeDir : string =
    Environment.GetFolderPath Environment.SpecialFolder.UserProfile

[<EntryPoint>]
let main (_) =
    chromeDir <- sprintf "%s/dev/chromedriver-2.44" homeDir
    start chrome

    Feeds.all ()

    run()
    quit()

    failedCount
