module ``Twitter Processor Tests``

    open NUnit.Framework
    open FsUnit
    open System
    open System.IO

    open Server.Articles.Data
    open Server.SourceType
    open Server.Feeds
    open Server.FeedsProcessing.Download
    open Server.FeedsProcessing.Twitter

    [<Test>]
    let ``processFeed twitter timeline`` () =
        let downloadedFeed = DownloadedFeed <| File.ReadAllText("../../../resources/twitter.timeline.json")


        let result = processTweets (TwitterHandle "its_damo") downloadedFeed


        match result with
        | Error _ -> Assert.Fail("Expected success")
        | Ok records ->
            let expectedTimeUtc = new DateTime(2018, 04, 12, 15, 34, 05, DateTimeKind.Utc)

            List.length records |> should equal 6
            List.head records |> should equal { Title = None
                                                Link = Some "https://twitter.com/its_damo"
                                                Content = "I'm really liking F# so far."
                                                Date = Some <| expectedTimeUtc.ToLocalTime()
                                                Source = Social
                                              }
