module SoManyFeedsServer.UserArticlesDataGateway

open SoManyFeedsServer
open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.DataSource
open FSharp.Data.Sql


let listRecentUnreadArticles (userId : int64) (maybeFeedId : int64 option) : AsyncResult<ArticleRecord seq> =
    dataAccessOperation (fun ctx ->
        let userReadArticleIds =
            query {
                for readArticle in ctx.Public.ReadArticles do
                where (readArticle.UserId = userId)
                select readArticle.ArticleId
            }

        let feedIds =
            match maybeFeedId with
            | Some feedId ->
                query {
                    for feed in ctx.Public.Feeds do
                    where (feed.Id = feedId)
                    select feed.Id
                }
            | None ->
                query {
                    for feed in ctx.Public.Feeds do
                    select feed.Id
                }

        query {
            for article in ctx.Public.Articles do

            join feed in ctx.Public.Feeds on (article.FeedUrl = feed.Url)
            join user in ctx.Public.Users on (feed.UserId = user.Id)

            where (user.Id = userId && (article.Id |<>| userReadArticleIds) && (feed.Id |=| feedIds))

            sortByDescending article.Date
            take 20

            select article
        }
        |> Seq.map ArticlesDataGateway.entityToRecord
    )


type ReadArticleRecord =
    { UserId : int64
      ArticleId : int64
    }


let createReadArticle (record : ReadArticleRecord) : AsyncResult<unit> =
    dataAccessOperation (fun ctx ->
        let entity = ctx.Public.ReadArticles.Create ()
        entity.UserId <- record.UserId
        entity.ArticleId <- record.ArticleId
        ctx.SubmitUpdates ()
    )


let deleteReadArticle (record : ReadArticleRecord) : AsyncResult<unit> =
    dataAccessOperation (fun ctx ->
        query {
            for readArticle in ctx.Public.ReadArticles do
            where (readArticle.UserId = record.UserId && readArticle.ArticleId = record.ArticleId)
            take 1
        }
        |> Seq.map (fun e -> e.Delete())
        |> ignore

        ctx.SubmitUpdates()
    )
