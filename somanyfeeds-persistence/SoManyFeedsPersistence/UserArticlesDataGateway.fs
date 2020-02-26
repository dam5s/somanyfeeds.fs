module SoManyFeedsPersistence.UserArticlesDataGateway

open FSharp.Data.Sql
open SoManyFeedsPersistence.ArticlesDataGateway
open SoManyFeedsPersistence.DataSource


let private readArticleIds (ctx: DataContext) userId =
    query {
        for readArticle in ctx.Public.ReadArticles do
        where (readArticle.UserId = userId)
        select readArticle.ArticleId
    }

let private bookmarkedArticleIds (ctx: DataContext) userId =
    query {
        for bookmark in ctx.Public.Bookmarks do
        where (bookmark.UserId = userId)
        select bookmark.ArticleId
    }

let private oneFeedId (ctx: DataContext) feedId =
    query {
        for feed in ctx.Public.Feeds do
        where (feed.Id = feedId)
        select feed.Id
    }

let private allFeedIds (ctx: DataContext) =
    query {
        for feed in ctx.Public.Feeds do
        select feed.Id
    }



let listRecentUnreadArticles (userId: int64) (maybeFeedId: int64 option): AsyncResult<ArticleRecord seq> =
    dataAccessOperation (fun ctx ->
        let feedIds =
            match maybeFeedId with
            | Some feedId -> oneFeedId ctx feedId
            | None -> allFeedIds ctx

        query {
            for article in ctx.Public.Articles do

            join feed in ctx.Public.Feeds on (article.FeedUrl = feed.Url)
            join user in ctx.Public.Users on (feed.UserId = user.Id)

            where (user.Id = userId
                  && (article.Id |<>| readArticleIds ctx userId)
                  && (article.Id |<>| bookmarkedArticleIds ctx userId)
                  && (feed.Id |=| feedIds)
                  )

            sortByDescending article.Date
            take 20

            select article
        }
        |> Seq.map ArticlesDataGateway.entityToRecord
    )


let listBookmarks userId: AsyncResult<ArticleRecord seq> =
    dataAccessOperation (fun ctx ->
        query {
            for article in ctx.Public.Articles do
            join bookmark in ctx.Public.Bookmarks on (article.Id = bookmark.ArticleId)
            where (bookmark.UserId = userId)

            sortByDescending article.Date
            select article
        }
        |> Seq.map ArticlesDataGateway.entityToRecord
    )


type UserArticleRecord =
    { UserId: int64
      ArticleId: int64
    }


let createReadArticle record: AsyncResult<unit> =
    dataAccessOperation (fun ctx ->
        let entity = ctx.Public.ReadArticles.Create()
        entity.UserId <- record.UserId
        entity.ArticleId <- record.ArticleId
        ctx.SubmitUpdates()
    )


let deleteReadArticle record: AsyncResult<unit> =
    dataAccessOperation (fun ctx ->
        query {
            for readArticle in ctx.Public.ReadArticles do
            where (readArticle.UserId = record.UserId && readArticle.ArticleId = record.ArticleId)
            take 1
        }
        |> Seq.iter (fun e -> e.Delete())

        ctx.SubmitUpdates()
    )


let createBookmark record: AsyncResult<unit> =
    dataAccessOperation (fun ctx ->
        let entity = ctx.Public.Bookmarks.Create()
        entity.UserId <- record.UserId
        entity.ArticleId <- record.ArticleId
        ctx.SubmitUpdates()
    )


let deleteBookmark record: AsyncResult<unit> =
    dataAccessOperation (fun ctx ->
        query {
            for bookmark in ctx.Public.Bookmarks do
            where (bookmark.UserId = record.UserId && bookmark.ArticleId = record.ArticleId)
            take 1
        }
        |> Seq.iter (fun e -> e.Delete())

        ctx.SubmitUpdates()
    )
