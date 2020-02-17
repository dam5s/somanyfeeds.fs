module IntegrationTests.Feeds

open System.Threading
open canopy.classic
open canopy.runner.classic
open Microsoft.AspNetCore.Hosting

let private tokenSource = new CancellationTokenSource()
let private webHostBuilder = Program.webHostBuilder()

let all() =
    context "Feeds"

    before (fun () ->
        webHostBuilder
            .UseUrls("http://localhost:9090")
            .Build()
            .RunAsync(tokenSource.Token) |> ignore
    )

    after (fun () ->
        tokenSource.Cancel()
    )

    "CRUD" &&& fun _ ->
        executeAllSql
            [ "delete from feeds"
              "delete from users" ]

        url "http://localhost:9090"

        expectToFind "h1" "Welcome"
        click "Read"

        expectToFind "h1" "Authentication required"
        click "Sign up now"

        expectToFind "h1" "Registration"
        "input[name='name']" << "Damo"
        "input[name='email']" << "damo@example.com"
        "input[name='password']" << "supersecret"
        "input[name='passwordConfirmation']" << "supersecret"
        click "Sign up"

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
