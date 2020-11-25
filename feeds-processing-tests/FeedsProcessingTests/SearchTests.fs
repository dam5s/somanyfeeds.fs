module ``Search Tests``

open FeedsProcessingTests.DownloadSupport
open FeedsProcessing.Search
open FeedsProcessing.Download
open FeedsProcessing.Xml
open FsUnit
open NUnit.Framework

[<Test>]
let ``with an xml feed`` () =
    let download =
        "../../../../feeds-processing-tests/Resources/test-samples/github.xml"
        |> Download.fromFilePath

    let result = search download

    match result with
    | WebPageMatch _ ->
        Assert.Fail "should be a feed match"
    | FeedMatch metadata ->
        metadata.Title |> should equal "dam5s’s Activity"
        metadata.Description |> should equal ""


[<Test>]
let ``with unsupported HTML`` () =
    let download = { Url = Url "file://some/where"; Content = "not quite html" }

    let result = search download

    result |> should equal (WebPageMatch [])


[<Test>]
let ``with HTML from Le Monde`` () =
    let download =
        "../../../../feeds-processing-tests/Resources/test-samples/le-monde.html"
        |> Download.fromFilePath

    let result = search download

    match result with
    | FeedMatch _ -> failwith "Expected a web page match"
    | WebPageMatch urls -> 
        urls 
        |> Seq.toList  
        |> should equal [ Url "https://www.lemonde.fr/rss/une.xml" ]
