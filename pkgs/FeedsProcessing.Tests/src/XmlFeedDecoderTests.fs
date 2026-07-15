module ``XmlFeedDecoder Tests``

open FeedsProcessing.XmlFeedDecoder
open Microsoft.Extensions.Logging.Abstractions
open Xunit
open System
open Time

open FeedsProcessing.Article
open FeedsProcessingTests.DownloadSupport

let testXmlFeedDecoder = XmlFeedDecoder(NullLogger<XmlFeedDecoder>.Instance)

[<Fact>]
let ``with unsupported XML`` () =
    task {
        let download = Download.fromContent "<foo>Not quite expected xml content</foo>"

        let! result = testXmlFeedDecoder.TryDecodeAsync download

        match result with
        | Ok _ -> failwith "Expected failure"
        | Error err -> Assert.True(List.length err > 0, "Expected at least one error")
    }

[<Fact>]
let ``with github Atom XML`` () =
    task {
        let download =
            "../../../../FeedsProcessing.Tests/resources/test-samples/github.xml"
            |> Download.fromFilePath

        let! result = testXmlFeedDecoder.TryDecodeAsync download

        match result with
        | Error _ -> failwith "Expected success"
        | Ok records ->
            Assert.Equal(7, List.length records)

            let article = List.head records

            let expectedTimeUtc =
                (2018, 04, 14, 21, 30, 17, TimeSpan.Zero)
                |> DateTimeOffset
                |> Posix.fromDateTimeOffset

            Assert.Equal(Some "dam5s pushed somanyfeeds.fs", Article.title article)

            Assert.Equal(Some "https://github.com/dam5s/somanyfeeds.fs", Article.link article)

            Assert.Equal("<p>Hello from the content</p>", Article.content article)
            Assert.Equal(Some expectedTimeUtc, Article.date article)
    }


[<Fact>]
let ``with Dualshock Atom XML`` () =
    task {
        let downloaded =
            "../../../../FeedsProcessing.Tests/resources/test-samples/dualshock.xml"
            |> Download.fromFilePath

        let! result = testXmlFeedDecoder.TryDecodeAsync downloaded

        match result with
        | Error _ -> failwith "Expected success"
        | Ok records -> Assert.Equal(10, List.length records)
    }


[<Fact>]
let ``with RSS XML`` () =
    task {
        let downloadedFeed =
            "../../../../FeedsProcessing/resources/samples/rss.sample.xml"
            |> Download.fromFilePath

        let! result = testXmlFeedDecoder.TryDecodeAsync downloadedFeed

        match result with
        | Error _ -> failwith "Expected success"
        | Ok records ->
            Assert.Equal(6, List.length records)

            let firstArticle = List.head records

            let expectedTimeUtc =
                (2016, 09, 20, 12, 54, 44, TimeSpan.Zero)
                |> DateTimeOffset
                |> Posix.fromDateTimeOffset

            Assert.Equal(Some "First title!", Article.title firstArticle)

            Assert.Equal(Some "https://medium.com/@its_damo/first", Article.link firstArticle)

            Assert.Equal("<p>This is the content in encoded tag</p>", Article.content firstArticle)

            Assert.Equal(Some expectedTimeUtc, Article.date firstArticle)

            let secondArticle = List.item 1 records
            Assert.Equal(Some "Second title!", Article.title secondArticle)

            Assert.Equal(Some "https://medium.com/@its_damo/second", Article.link secondArticle)

            Assert.Equal("<p>This is the content in description tag</p>", Article.content secondArticle)

            Assert.Equal(Some expectedTimeUtc, Article.date secondArticle)
    }

[<Fact>]
let ``with mastodon RSS`` () =
    task {
        let downloadedFeed =
            "../../../../FeedsProcessing.Tests/resources/test-samples/mastodon.xml"
            |> Download.fromFilePath

        let! result = testXmlFeedDecoder.TryDecodeAsync downloadedFeed

        match result with
        | Error _ -> failwith "Expected success"
        | Ok records ->
            Assert.Equal(19, List.length records)

            let mediaAt index =
                records |> List.item index |> Article.media

            let expectedMediaNoDescription =
                { Url =
                    "https://mastodon.kleph.eu/system/media_attachments/files/000/055/839/original/4874168bf454bddb.jpg"
                  Description = "" }

            Assert.Equal(Some expectedMediaNoDescription, mediaAt 17)

            let expectedMediaAndDescription =
                { Url =
                    "https://mastodon.kleph.eu/system/media_attachments/files/108/207/041/262/751/249/original/1e8a0ccac5f59165.jpg"
                  Description = "Mountain landscape at sunset with bicycle on the foreground." }

            Assert.Equal(Some expectedMediaAndDescription, mediaAt 13)
    }
