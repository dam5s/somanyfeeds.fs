module ``Twitter Processor Tests``

    open NUnit.Framework
    open FsUnit
    open System
    open System.IO

    open FeedsProcessing.Article
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
            let expectedTimeUtc = new DateTimeOffset (2018, 04, 12, 15, 34, 05, TimeSpan.Zero)

            List.length records |> should equal 6
            List.head records |> should equal { Title = None
                                                Link = Some "https://twitter.com/its_damo"
                                                Content = "I'm really liking F# so far."
                                                Date = Some <| expectedTimeUtc.ToLocalTime ()
                                              }
