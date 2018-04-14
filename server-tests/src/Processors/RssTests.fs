module ``Rss Processor Tests``

    open NUnit.Framework
    open FsUnit
    open System
    open System.IO
    open Server.Feeds
    open Server
    open ArticlesData

    [<Test>]
    let ``processFeed with standard medium xml`` () =
        let downloadFunction (url: string) : string =
            url |> should equal "http://example.com/medium/rss"
            File.ReadAllText("../../../resources/medium.rss.xml")

        let medium: Feed = { Name = "Medium"
                             Slug = "social"
                             Info = "http://example.com/medium/rss"
                             Type = FeedType.Rss
                           }


        let result = Server.Processors.Rss.processFeed downloadFunction medium


        match result with
        | Error _ -> Assert.Fail("Expected success")
        | Ok records ->
            let expectedTimeUtc = new DateTime(2016, 09, 20, 12, 54, 44, DateTimeKind.Utc)

            List.length records |> should equal 5
            List.head records |> should equal { Title = Some "First title!"
                                                Link = Some "https://medium.com/@its_damo/first"
                                                Content = "<p>This is the content</p>"
                                                Date = Some <| expectedTimeUtc.ToLocalTime()
                                                Source = "social"
                                              }
