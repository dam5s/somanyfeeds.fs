module SoManyFeedsServer.UserArticlesService

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.Authentication
open SoManyFeedsServer.FeedsDataGateway
open SoManyFeedsServer.DataSource


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
        let! feeds = FeedsDataGateway.listFeeds dataSource user.Id
        let! articles = UserArticlesDataGateway.listRecentUnreadArticles dataSource user.Id
        return articlesWithFeeds feeds articles
    }
