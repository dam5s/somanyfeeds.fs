module SoManyFeedsPersistence.UserArticlesService

open SoManyFeedsPersistence.ArticlesDataGateway
open SoManyFeedsPersistence.FeedsDataGateway
open SoManyFeedsDomain.User


let listRecent (user: User) maybeFeedId: AsyncResult<FeedRecord seq * ArticleRecord seq> =
    asyncResult {
        let! feeds = FeedsDataGateway.listFeeds user.Id
        let! articles = UserArticlesDataGateway.listRecentUnreadArticles user.Id maybeFeedId

        return feeds, articles
    }

let listBookmarks (user: User): AsyncResult<FeedRecord seq * ArticleRecord seq> =
    asyncResult {
        let! feeds = FeedsDataGateway.listFeeds user.Id
        let! articles = UserArticlesDataGateway.listBookmarks user.Id

        return feeds, articles
    }
