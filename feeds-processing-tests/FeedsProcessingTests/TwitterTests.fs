module ``Twitter Processor Tests``

open FeedsProcessingTests.DownloadSupport
open FeedsProcessing.Article
open FeedsProcessing.Feeds
open FeedsProcessing.Twitter
open FeedsProcessing
open NUnit.Framework
open FsUnit
open System
open Time


[<Test>]
let ``processFeed twitter timeline``() =
    let downloadedFeed =
        "../../../../feeds-processing/Resources/samples/twitter.timeline.sample.json"
        |> Download.fromFilePath


    let result = processTweets (TwitterHandle "its_damo") downloadedFeed


    match result with
    | Error _ -> Assert.Fail "Expected success"
    | Ok records ->
        List.length records |> should equal 6

        let article = List.head records
        let expectedTimeUtc = (2018, 04, 12, 15, 34, 05, TimeSpan.Zero)
                              |> DateTimeOffset
                              |> Posix.fromDateTimeOffset

        Article.title article |> should equal None
        Article.link article |> should equal (Some "https://twitter.com/its_damo")
        Article.content article |> should equal "I'm really liking F# so far."
        Article.date article |> should equal (Some expectedTimeUtc)
