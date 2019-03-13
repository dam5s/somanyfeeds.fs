module SoManyFeedsServer.UserArticlesService

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.Authentication
open SoManyFeedsServer.FeedsDataGateway
open SoManyFeedsServer.DataSource


let private articlesWithFeeds (feeds : FeedRecord seq) (articles : ArticleRecord seq) : (FeedRecord option * ArticleRecord) seq =
    articles
    |> Seq.map (fun article ->
        let feed =
            feeds
            |> Seq.tryFind (fun feed -> feed.Url = article.FeedUrl)

        feed, article
    )


let listRecent (dataContext : DataContext) (user : User) : AsyncResult<(FeedRecord option * ArticleRecord) seq> =
    asyncResult {
        let! feeds = FeedsDataGateway.listFeeds dataContext user.Id
        let! articles = UserArticlesDataGateway.listRecentUnreadArticles dataContext user.Id
        return articlesWithFeeds feeds articles
    }
