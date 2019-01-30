module ``Twitter Processor Tests``

    open NUnit.Framework
    open FsUnit
    open System
    open System.IO

    open FeedsProcessing
    open FeedsProcessing.Download
    open FeedsProcessing.Feeds
    open FeedsProcessing.Twitter

    [<Test>]
    let ``processFeed twitter timeline`` () =
        let downloadedFeed = DownloadedFeed <| File.ReadAllText "../../../../feeds-processing/Resources/samples/twitter.timeline.sample"


        let result = processTweets (TwitterHandle "its_damo") downloadedFeed


        match result with
        | Error _ -> Assert.Fail "Expected success"
        | Ok records ->
            List.length records |> should equal 6

            let article = List.head records
            let expectedTimeUtc = new DateTimeOffset (2018, 04, 12, 15, 34, 05, TimeSpan.Zero)
            Article.title article |> should equal None
            Article.link article |> should equal (Some "https://twitter.com/its_damo")
            Article.content article |> should equal "I'm really liking F# so far."
            Article.date article |> should equal (Some <| expectedTimeUtc.ToLocalTime ())
