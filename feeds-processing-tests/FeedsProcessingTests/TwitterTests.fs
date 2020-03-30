module ``Twitter Processor Tests``

open FeedsProcessing
open FeedsProcessing.Download
open FeedsProcessing.Feeds
open FeedsProcessing.Twitter
open FsUnit

open NUnit.Framework
open System
open System.IO
open Time


[<Test>]
let ``processFeed twitter timeline``() =
    let downloadedFeed = "../../../../feeds-processing/Resources/samples/twitter.timeline.sample.json"
                         |> File.ReadAllText
                         |> DownloadedFeed


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
