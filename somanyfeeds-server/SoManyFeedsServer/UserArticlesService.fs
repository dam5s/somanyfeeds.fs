module SoManyFeedsServer.UserArticlesService

open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.Authentication
open SoManyFeedsServer.FeedsDataGateway


let listRecent (user : User) (maybeFeedId : int64 option) : AsyncResult<FeedRecord seq * ArticleRecord seq> =
    asyncResult {
        let! feeds = FeedsDataGateway.listFeeds user.Id
        let! articles = UserArticlesDataGateway.listRecentUnreadArticles user.Id maybeFeedId

        return feeds, articles
    }
