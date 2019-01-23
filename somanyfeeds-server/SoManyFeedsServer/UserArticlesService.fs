module SoManyFeedsServer.UserArticlesService

open SoManyFeedsServer.ArticlesPersistence
open SoManyFeedsServer.Authentication
open SoManyFeedsServer.FeedsPersistence
open SoManyFeedsServer.DataSource


let private recentArticlesForFeeds (dataSource : DataSource) (feeds : FeedRecord list) : AsyncResult<ArticleRecord list> =
    feeds
    |> List.map (fun feed -> feed.Url)
    |> ArticlesPersistence.listRecentArticles dataSource


let private articlesWithFeeds (feeds : FeedRecord list) (articles : ArticleRecord list) : (FeedRecord * ArticleRecord) list =
    articles
    |> List.map (fun article ->
        let feed =
            feeds
            |> List.find (fun feed -> feed.Url = article.FeedUrl)

        feed, article
    )


let listRecent (dataSource : DataSource) (user : User) : AsyncResult<(FeedRecord * ArticleRecord) list> =
    asyncResult {
        let! feeds = FeedsPersistence.listFeeds dataSource user.Id
        let! articles = recentArticlesForFeeds dataSource feeds
        return articlesWithFeeds feeds articles
    }
