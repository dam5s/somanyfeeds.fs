module IntegrationTests.Feeds

open Assertions
open canopy.runner.classic
open canopy.classic
open SoManyFeedsServer


let all () =
    "test feeds CRUD" &&& fun _ ->
        url <| sprintf "http://localhost:%d" Config.port

        click "follow some feeds"

        expectToFind "h1" "Manage your subscriptions"
