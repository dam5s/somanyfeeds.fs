module ``Atom Processor Tests``

    open NUnit.Framework
    open FsUnit
    open System
    open System.IO

    open Server.Articles.Data
    open Server.SourceType
    open Server.FeedsProcessing.Download
    open Server.FeedsProcessing.Atom

    [<Test>]
    let ``processFeed with standard github xml`` () =
        let downloaded = DownloadedFeed <| File.ReadAllText("../../../resources/github.atom.xml")


        let result = processAtomFeed Code downloaded


        match result with
        | Error _ -> Assert.Fail("Expected success")
        | Ok records ->
            let expectedTimeUtc = new DateTime(2018, 04, 14, 21, 30, 17, DateTimeKind.Utc)

            List.length records |> should equal 7
            List.head records |> should equal { Title = Some "dam5s pushed to master in dam5s/somanyfeeds.fs"
                                                Link = Some "https://github.com/dam5s/somanyfeeds.fs"
                                                Content = "<p>Hello from the content</p>"
                                                Date = Some <| expectedTimeUtc.ToLocalTime()
                                                Source = Code
                                              }
