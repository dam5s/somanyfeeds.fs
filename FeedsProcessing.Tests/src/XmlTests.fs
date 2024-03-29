module ``Xml Processor Tests``

open FsUnit
open FsUnitTyped
open NUnit.Framework
open System
open Time

open FeedsProcessing.Article
open FeedsProcessing.Xml
open FeedsProcessingTests.DownloadSupport

[<Test>]
let ``with unsupported XML`` () =
    let download = Download.fromContent "<foo>Not quite expected xml content</foo>"

    let result = Xml.processFeed download

    match result with
    | Ok _ -> Assert.Fail "Expected failure"
    | Error err ->
        String.length err.Message |> shouldBeGreaterThan 0
        List.length err.Exceptions |> shouldEqual 1


[<Test>]
let ``with github Atom XML`` () =
    let download =
        "../../../../FeedsProcessing.Tests/resources/test-samples/github.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed download

    match result with
    | Error _ -> Assert.Fail "Expected success"
    | Ok records ->
        List.length records |> should equal 7

        let article = List.head records

        let expectedTimeUtc =
            (2018, 04, 14, 21, 30, 17, TimeSpan.Zero)
            |> DateTimeOffset
            |> Posix.fromDateTimeOffset

        Article.title article
        |> should equal (Some "dam5s pushed to master in dam5s/somanyfeeds.fs")

        Article.link article
        |> should equal (Some "https://github.com/dam5s/somanyfeeds.fs")

        Article.content article |> should equal "<p>Hello from the content</p>"
        Article.date article |> should equal (Some expectedTimeUtc)


[<Test>]
let ``with Dualshock Atom XML`` () =
    let downloaded =
        "../../../../FeedsProcessing.Tests/resources/test-samples/dualshock.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed downloaded

    match result with
    | Error _ -> Assert.Fail "Expected success"
    | Ok records -> List.length records |> should equal 10


[<Test>]
let ``with RSS XML`` () =
    let downloadedFeed =
        "../../../../FeedsProcessing/resources/samples/rss.sample.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed downloadedFeed

    match result with
    | Error _ -> Assert.Fail "Expected success"
    | Ok records ->
        List.length records |> should equal 6

        let firstArticle = List.head records

        let expectedTimeUtc =
            (2016, 09, 20, 12, 54, 44, TimeSpan.Zero)
            |> DateTimeOffset
            |> Posix.fromDateTimeOffset

        Article.title firstArticle |> should equal (Some "First title!")

        Article.link firstArticle
        |> should equal (Some "https://medium.com/@its_damo/first")

        Article.content firstArticle
        |> should equal "<p>This is the content in encoded tag</p>"

        Article.date firstArticle |> should equal (Some expectedTimeUtc)

        let secondArticle = List.item 1 records
        Article.title secondArticle |> should equal (Some "Second title!")

        Article.link secondArticle
        |> should equal (Some "https://medium.com/@its_damo/second")

        Article.content secondArticle
        |> should equal "<p>This is the content in description tag</p>"

        Article.date secondArticle |> should equal (Some expectedTimeUtc)

[<Test>]
let ``with mastodon RSS`` () =
    let downloadedFeed =
        "../../../../FeedsProcessing.Tests/resources/test-samples/mastodon.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed downloadedFeed

    match result with
    | Error _ -> Assert.Fail "Expected success"
    | Ok records ->
        List.length records |> should equal 19

        let mediaAt index =
            records |> List.item index |> Article.media

        let expectedMediaNoDescription =
            { Url = "https://mastodon.kleph.eu/system/media_attachments/files/000/055/839/original/4874168bf454bddb.jpg"
              Description = "" }

        mediaAt 17 |> should equal (Some expectedMediaNoDescription)

        let expectedMediaAndDescription =
            { Url =
                "https://mastodon.kleph.eu/system/media_attachments/files/108/207/041/262/751/249/original/1e8a0ccac5f59165.jpg"
              Description = "Mountain landscape at sunset with bicycle on the foreground." }

        mediaAt 13 |> should equal (Some expectedMediaAndDescription)

[<Test>]
let ``processFeed with slashdot RDF XML`` () =
    let downloadedFeed =
        "../../../../FeedsProcessing.Tests/resources/test-samples/slashdot.xml"
        |> Download.fromFilePath

    let result = Xml.processFeed downloadedFeed

    match result with
    | Error _ -> Assert.Fail "Expected success"
    | Ok records ->
        List.length records |> should equal 15

        let article = List.head records

        let expectedTimeUtc =
            (2018, 12, 28, 20, 55, 0, TimeSpan.Zero)
            |> DateTimeOffset
            |> Posix.fromDateTimeOffset

        Article.title article
        |> should equal (Some "Netflix Permanently Pulls iTunes Billing For New and Returning Users")

        Article.link article
        |> should
            equal
            (Some
                "https://news.slashdot.org/story/18/12/28/2054254/netflix-permanently-pulls-itunes-billing-for-new-and-returning-users?utm_source=rss1.0mainlinkanon&utm_medium=feed")

        Article.content article
        |> should equal "An anonymous reader shares a report: Netflix is further distancing itself..."

        Article.date article |> should equal (Some expectedTimeUtc)
