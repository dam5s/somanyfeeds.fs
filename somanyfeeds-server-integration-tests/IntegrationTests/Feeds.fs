module IntegrationTests.Feeds

open SoManyFeedsServer
open System.Threading
open canopy.classic
open canopy.runner.classic

let mutable private tokenSource: CancellationTokenSource option =
    None

let all() =
    context "Feeds"

    before (fun () ->
        LoggingConfig.configure()

        let config = SoManyFeedsServer.WebConfig.create
        let webPart = SoManyFeedsServer.WebApp.webPart

        tokenSource <- Some <| WebServerSupport.start config webPart
    )

    after (fun () ->
        tokenSource
        |> Option.map (fun src -> src.Cancel())
        |> ignore
    )

    "CRUD" &&& fun _ ->
        executeAllSql
            [ "delete from feeds"
              "delete from users" ]

        url (sprintf "http://localhost:%d" WebConfig.port)

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
