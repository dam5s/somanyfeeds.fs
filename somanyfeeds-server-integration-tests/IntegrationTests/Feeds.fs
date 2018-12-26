module IntegrationTests.Feeds

open Assertions
open canopy.runner.classic
open canopy.classic


let all () =
    "test feeds CRUD" &&& fun _ ->
        url "http://localhost:8080"

        click "follow some feeds"

        expectToFind "h1" "Manage your subscriptions"
