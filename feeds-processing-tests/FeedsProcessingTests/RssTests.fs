module ``Rss Processor Tests``

    open NUnit.Framework
    open FsUnit
    open System
    open System.IO

    open FeedsProcessing.Article
    open FeedsProcessing.Download
    open FeedsProcessing.Xml

    [<Test>]
    let ``processFeed with standard medium xml`` () =
        let downloadedFeed = DownloadedFeed <| File.ReadAllText("../../../../feeds-processing/Resources/samples/medium.rss.sample")


        let result = processXmlFeed downloadedFeed


        match result with
        | Error _ -> Assert.Fail("Expected success")
        | Ok records ->
            let expectedTimeUtc = new DateTimeOffset(2016, 09, 20, 12, 54, 44, TimeSpan.Zero)

            List.length records |> should equal 5
            List.head records |> should equal { Title = Some "First title!"
                                                Link = Some "https://medium.com/@its_damo/first"
                                                Content = "<p>This is the content</p>"
                                                Date = Some expectedTimeUtc
                                              }
