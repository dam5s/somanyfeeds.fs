module ``Search Tests``

open FeedsProcessingTests.DownloadSupport
open FeedsProcessing.Search
open FeedsProcessing.Download
open Xunit

[<Fact>]
let ``with an xml feed`` () =
    let download =
        "../../../../FeedsProcessing.Tests/resources/test-samples/github.xml"
        |> Download.fromFilePath

    let result = search download

    match result with
    | WebPageMatch _ -> failwith "should be a feed match"
    | FeedMatch metadata ->
        Assert.Equal("GitHub Public Timeline Feed", metadata.Title)
        Assert.Equal("", metadata.Description)

[<Fact>]
let ``with unsupported HTML`` () =
    let download =
        { Url = Url "file://some/where"
          Content = "not quite html" }

    let result = search download

    Assert.Equal(WebPageMatch [], result)

[<Fact>]
let ``with HTML from Le Monde`` () =
    let download =
        "../../../../FeedsProcessing.Tests/resources/test-samples/le-monde.html"
        |> Download.fromFilePath

    let result = search download

    match result with
    | FeedMatch _ -> failwith "Expected a web page match"
    | WebPageMatch urls -> Assert.Equal<Url>([ Url "https://www.lemonde.fr/rss/une.xml" ], Seq.toList urls)
