module UserArticlesDataGateway

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.DataSource


let listRecentUnreadArticles (dataSource : DataSource) (userId : int64) : AsyncResult<ArticleRecord list> =
        let bindings =
            [ Binding ("@UserId", userId) ]

        let sql =
            """ select a.id, a.url, a.title, a.feed_url, a.content, a.date
                from articles a
                join feeds f on f.url = a.feed_url
                join users u on u.id = f.user_id
                left join read_articles r on r.user_id = f.user_id and r.article_id = a.id
                where u.id = @UserId
                and r.article_id is null
                order by date desc
                limit 20
            """

        findAll dataSource sql bindings mapArticle


type ReadArticleRecord =
    { UserId : int64
      ArticleId : int64
    }


let createReadArticle (dataSource : DataSource) (record : ReadArticleRecord) : AsyncResult<unit> =
    let bindings =
        [
        Binding ("@UserId", record.UserId)
        Binding ("@ArticleId", record.ArticleId)
        ]

    update dataSource "insert into read_articles (user_id, article_id) values (@UserId, @ArticleId)" bindings
    |> AsyncResult.map (always ())


let deleteReadArticle (dataSource : DataSource) (record : ReadArticleRecord) : AsyncResult<unit> =
    let bindings =
        [
        Binding ("@UserId", record.UserId)
        Binding ("@ArticleId", record.ArticleId)
        ]

    update dataSource "delete from read_articles where user_id = @UserId and article_id = @ArticleId" bindings
    |> AsyncResult.map (always ())
