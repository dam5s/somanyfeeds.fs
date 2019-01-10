module IntegrationTests.Feeds

open canopy.runner.classic
open canopy.classic
open SoManyFeedsServer


let all () =
    before (fun _ ->
        executeSql "truncate feeds"
    )

    "Feeds CRUD" &&& fun _ ->
        url <| sprintf "http://localhost:%d" Config.port

        expectToFind "h1" "Welcome"
        click "Read"

        expectToFind "h1" "Authentication required"
        "input[name='email']" << "damo@example.com"
        "input[name='password']" << "supersecret"
        click "Sign in"

        expectToFind "h2" "ARTICLES"

        click "Manage"
        expectToFind "h2" "FEEDS"
        expectToFind "p" "You have not subscribed to any feeds yet."
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
        expectToFind "h3" "Unsubscribe"

        click "Yes, unsubscribe"
        count ".card-list .card" 1
        expectToFind ".card dd" "My Test Feed"
