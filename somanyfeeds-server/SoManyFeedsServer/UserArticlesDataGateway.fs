module UserArticlesDataGateway

open SoManyFeedsServer
open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.DataSource
open FSharp.Data.Sql


let listRecentUnreadArticles (dataContext : DataContext) (userId : int64) : AsyncResult<ArticleRecord seq> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            let userReadArticleIds =
                query {
                    for readArticle in ctx.Public.ReadArticles do
                    where (readArticle.UserId = userId)
                    select readArticle.ArticleId
                }

            query {
                for article in ctx.Public.Articles do

                join feed in ctx.Public.Feeds on (article.FeedUrl = feed.Url)
                join user in ctx.Public.Users on (feed.UserId = user.Id)

                where (user.Id = userId && (article.Id |<>| userReadArticleIds))

                sortByDescending article.Date
                take 20

                select article
            }
            |> Seq.map ArticlesDataGateway.entityToRecord
        }
    }


type ReadArticleRecord =
    { UserId : int64
      ArticleId : int64
    }


let createReadArticle (dataContext : DataContext) (record : ReadArticleRecord) : AsyncResult<unit> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            let entity = ctx.Public.ReadArticles.Create ()
            entity.UserId <- record.UserId
            entity.ArticleId <- record.ArticleId
            ctx.SubmitUpdates ()
        }
    }


let deleteReadArticle (dataContext : DataContext) (record : ReadArticleRecord) : AsyncResult<unit> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            query {
                for readArticle in ctx.Public.ReadArticles do
                where (readArticle.UserId = record.UserId && readArticle.ArticleId = record.ArticleId)
                take 1
            }
            |> Seq.map (fun e -> e.Delete())
            |> ignore

            ctx.SubmitUpdates()
        }
    }
