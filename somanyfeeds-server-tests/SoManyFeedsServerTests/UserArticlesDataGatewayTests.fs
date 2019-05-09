module ``UserArticlesDataGateway tests``

open NUnit.Framework
open FsUnit
open SoManyFeedsServer
open SoManyFeedsServer.ArticlesDataGateway

[<Test>]
let ``recent unread articles`` () =

    setTestDbConnectionString ()
    executeAllSql
        [
        "delete from read_articles"
        "delete from articles"
        "delete from feeds"
        "delete from users"
        """
            insert into users (id, email, name, password_hash) values
            (10, 'john@example.com', 'Johnny', ''),
            (11, 'jane@example.com', 'Jeanne', '')
        """
        """
            insert into feeds (id, user_id, name, url) values
            (101, 10, 'My Feed #1', 'http://example.com/my-feeds/1'),
            (102, 10, 'My Feed #2', 'http://example.com/my-feeds/2'),
            (103, 11, 'My Feed #2', 'http://example.com/my-feeds/2')
        """
        """
            insert into articles (id, url, title, feed_url, content, date) values
            (310, 'http://example.com/my-feeds/1/A', 'Article #1A', 'http://example.com/my-feeds/1', 'Oh Hai #1A', '2018-12-31 12:30:00'),
            (311, 'http://example.com/my-feeds/1/B', 'Article #1B', 'http://example.com/my-feeds/1', 'Oh Hai #1B', '2018-12-31 13:30:00'),
            (312, 'http://example.com/my-feeds/1/C', 'Article #1C', 'http://example.com/my-feeds/1', 'Oh Hai #1C', '2018-12-31 14:30:00'),
            (313, 'http://example.com/my-feeds/2/A', 'Article #2A', 'http://example.com/my-feeds/2', 'Oh Hai #2A', '2018-12-31 12:30:00'),
            (314, 'http://example.com/my-feeds/2/B', 'Article #2B', 'http://example.com/my-feeds/2', 'Oh Hai #2B', '2018-12-31 13:30:00')
        """
        """
            insert into read_articles (user_id, article_id) values
            (10, 311),
            (10, 314)
        """
        ]


    let articleIds (asyncRes : AsyncResult<ArticleRecord seq>) : int64 list =
        asyncRes
        |> Async.RunSynchronously
        |> Result.map (Seq.map (fun a -> a.Id) >> Seq.toList >> List.sort)
        |> Result.defaultValue []


    UserArticlesDataGateway.listRecentUnreadArticles (int64 10) None
    |> articleIds
    |> should equal [ 310 ; 312 ; 313 ]

    UserArticlesDataGateway.listRecentUnreadArticles (int64 11) None
    |> articleIds
    |> should equal [ 313 ; 314 ]

    UserArticlesDataGateway.listRecentUnreadArticles (int64 10) (Some (int64 101))
    |> articleIds
    |> should equal [ 310 ; 312 ]

    UserArticlesDataGateway.listRecentUnreadArticles (int64 12) None
    |> articleIds
    |> should equal []
