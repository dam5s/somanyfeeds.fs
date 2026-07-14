module ``Xml Processor Tests``

open Xunit
open System
open Time

open FeedsProcessing.Article
open FeedsProcessing.Xml
open FeedsProcessingTests.DownloadSupport

[<Fact>]
let ``with unsupported XML`` () =
    let download = Download.fromContent "<foo>Not quite expected xml content</foo>"

    let result = Xml.processFeed download

    match result with
    | Ok _ -> failwith "Expected failure"
    | Error err ->
        Assert.NotEqual(0, String.length err.Message)
        Assert.Equal(1, List.length err.Exceptions)

[<Fact>]
let ``with github Atom XML`` () =
    let download =
        "../../../../FeedsProcessing.Tests/resources/test-samples/github.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed download

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


[<Fact>]
let ``with Dualshock Atom XML`` () =
    let downloaded =
        "../../../../FeedsProcessing.Tests/resources/test-samples/dualshock.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed downloaded

    match result with
    | Error _ -> failwith "Expected success"
    | Ok records -> Assert.Equal(10, List.length records)


[<Fact>]
let ``with RSS XML`` () =
    let downloadedFeed =
        "../../../../FeedsProcessing/resources/samples/rss.sample.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed downloadedFeed

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

[<Fact>]
let ``with mastodon RSS`` () =
    let downloadedFeed =
        "../../../../FeedsProcessing.Tests/resources/test-samples/mastodon.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed downloadedFeed

    match result with
    | Error _ -> failwith "Expected success"
    | Ok records ->
        Assert.Equal(19, List.length records)

        let mediaAt index =
            records |> List.item index |> Article.media

        let expectedMediaNoDescription =
            { Url = "https://mastodon.kleph.eu/system/media_attachments/files/000/055/839/original/4874168bf454bddb.jpg"
              Description = "" }

        Assert.Equal(Some expectedMediaNoDescription, mediaAt 17)

        let expectedMediaAndDescription =
            { Url =
                "https://mastodon.kleph.eu/system/media_attachments/files/108/207/041/262/751/249/original/1e8a0ccac5f59165.jpg"
              Description = "Mountain landscape at sunset with bicycle on the foreground." }

        Assert.Equal(Some expectedMediaAndDescription, mediaAt 13)

[<Fact>]
let ``processFeed with slashdot RDF XML`` () =
    let downloadedFeed =
        "../../../../FeedsProcessing.Tests/resources/test-samples/slashdot.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed downloadedFeed

    match result with
    | Error _ -> failwith "Expected success"
    | Ok records ->
        Assert.Equal(15, List.length records)

        let article = List.head records

        let expectedTimeUtc =
            (2018, 12, 28, 20, 55, 0, TimeSpan.Zero)
            |> DateTimeOffset
            |> Posix.fromDateTimeOffset

        Assert.Equal(Some "Netflix Permanently Pulls iTunes Billing For New and Returning Users", Article.title article)

        Assert.Equal(
            Some
                "https://news.slashdot.org/story/18/12/28/2054254/netflix-permanently-pulls-itunes-billing-for-new-and-returning-users?utm_source=rss1.0mainlinkanon&utm_medium=feed",
            Article.link article
        )

        Assert.Equal(
            "An anonymous reader shares a report: Netflix is further distancing itself...",
            Article.content article
        )

        Assert.Equal(Some expectedTimeUtc, Article.date article)
