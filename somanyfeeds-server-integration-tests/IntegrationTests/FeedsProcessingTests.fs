module ``Feeds processing tests``

open FsUnitTyped.TopLevelOperators
open NUnit.Framework
open SoManyFeedsPersistence.FeedsProcessor

[<SetUp>]
let before() = 
    TestWebsite.start()

[<TearDown>]
let after() = 
    TestWebsite.stop()

[<Test>]
let ``test running background jobs``() =
    executeAllSql
        [
        "delete from feed_jobs"
        "delete from articles"
        "delete from feeds"
        "delete from users"
        "insert into users (id, email, name, password_hash) values (10, 'john@example.com', 'Johnny', '')"
        """ insert into feeds (id, user_id, name, url) values
            (101, 10, 'Rss Feed', 'http://localhost:9092/rss.xml'),
            (102, 10, 'Atom Feed', 'http://localhost:9092/atom.xml')
        """
        ]

    Async.RunSynchronously backgroundProcessingOnce

    queryDataContext (fun ctx -> query { for a in ctx.Public.Articles do select a.Id })
    |> Seq.length
    |> shouldEqual 13
