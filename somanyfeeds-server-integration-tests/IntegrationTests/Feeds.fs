module IntegrationTests.Feeds

open Assertions
open DatabaseSupport
open canopy.runner.classic
open canopy.classic
open SoManyFeedsServer


let all () =
    before (fun _ ->
        executeSql "truncate feeds"
    )

    "Feeds CRUD" &&& fun _ ->
        url <| sprintf "http://localhost:%d" Config.port

        click "follow some feeds"

        expectToFind "h1" "Manage your subscriptions"
        count ".card-list .card" 0

        "input[name='name']" << "My Test Feed"
        "input[name='url']" << "http://example.com/my/test/feed.rss"
        click "Subscribe"

        count ".card-list .card" 1
        expectToFind ".card dd" "My Test Feed"
        expectToFind ".card dd" "http://example.com/my/test/feed.rss"

        "input[name='name']" << "My Test Feed #2"
        "input[name='url']" << "http://example.com/my/test/feed-2.rss"
        click "Subscribe"

        count ".card-list .card" 2
        expectToFind ".card dd" "My Test Feed #2"
        expectToFind ".card dd" "http://example.com/my/test/feed-2.rss"

        click "Unsubscribe"
        click "Yes, unsubscribe"
        count ".card-list .card" 1
        expectToFind ".card dd" "My Test Feed"
