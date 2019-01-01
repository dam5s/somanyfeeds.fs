module ``Xml Processor Tests``

    open NUnit.Framework
    open FsUnit
    open FsUnitTyped
    open System
    open System.IO
    open FeedsProcessing.Article
    open FeedsProcessing.Download
    open FeedsProcessing.Xml


    [<Test>]
    let ``with unsupported XML``() =
        let downloaded = DownloadedFeed "<foo>Not quite expected xml content</foo>"


        let result = processXmlFeed downloaded


        match result with
        | Ok _ -> Assert.Fail("Expected failure")
        | Error msg ->
            String.length msg |> shouldBeGreaterThan 0


    [<Test>]
    let ``with github Atom XML``() =
        let downloaded = DownloadedFeed <| File.ReadAllText("../../../../feeds-processing/Resources/samples/github.atom.sample")


        let result = processXmlFeed downloaded


        match result with
        | Error _ -> Assert.Fail("Expected success")
        | Ok records ->
            let expectedTimeUtc = new DateTimeOffset(2018, 04, 14, 21, 30, 17, TimeSpan.Zero)

            List.length records |> should equal 7
            List.head records |> should equal { Title = Some "dam5s pushed to master in dam5s/somanyfeeds.fs"
                                                Link = Some "https://github.com/dam5s/somanyfeeds.fs"
                                                Content = "<p>Hello from the content</p>"
                                                Date = Some expectedTimeUtc
                                              }


    [<Test>]
    let ``with medium RSS XML``() =
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


    [<Test>]
    let ``processFeed with slashdot RDF XML``() =
        let downloadedFeed = DownloadedFeed <| File.ReadAllText("../../../../feeds-processing/Resources/samples/slashdot.rdf.sample")


        let result = processXmlFeed downloadedFeed


        match result with
        | Error _ -> Assert.Fail("Expected success")
        | Ok records ->
            let expectedTimeUtc = new DateTimeOffset(2018, 12, 28, 20, 55, 0, TimeSpan.Zero)
            List.length records |> should equal 15
            List.head records |> should equal { Title = Some "Netflix Permanently Pulls iTunes Billing For New and Returning Users"
                                                Link = Some "https://news.slashdot.org/story/18/12/28/2054254/netflix-permanently-pulls-itunes-billing-for-new-and-returning-users?utm_source=rss1.0mainlinkanon&utm_medium=feed"
                                                Content = "An anonymous reader shares a report: Netflix is further distancing itself..."
                                                Date = Some expectedTimeUtc
                                              }
