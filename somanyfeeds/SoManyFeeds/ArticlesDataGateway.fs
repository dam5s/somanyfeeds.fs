module SoManyFeeds.ArticlesDataGateway

open Time
open SoManyFeeds.DataSource


type ArticleRecord =
    { Id : int64
      Url : string
      Title : string
      FeedUrl : string
      Content : string
      Date : Posix option
    }


type ArticleFields =
    { Url : string
      Title : string
      FeedUrl : string
      Content : string
      Date : Posix option
    }


let entityToRecord (entity : ArticleEntity) : ArticleRecord =
    { Id = entity.Id
      Url = entity.Url
      Title = entity.Title
      FeedUrl = entity.FeedUrl
      Content = entity.Content
      Date = entity.Date |> Option.map Posix.fromDateTime
    }


let createOrUpdateArticle (fields : ArticleFields) : AsyncResult<ArticleRecord> =
    dataAccessOperation (fun ctx ->
        let maybeExisting =
            query {
                for a in ctx.Public.Articles do
                where (a.FeedUrl = fields.FeedUrl && a.Url = fields.Url)
                take 1
            }
            |> Seq.tryHead

        let entity =
            match maybeExisting with
            | Some e -> e
            | None -> ctx.Public.Articles.Create ()

        entity.Url <- fields.Url
        entity.FeedUrl <- fields.FeedUrl
        entity.Title <- fields.Title
        entity.Content <- fields.Content
        entity.Date <- fields.Date |> Option.map Posix.toDateTime

        ctx.SubmitUpdates ()
        entityToRecord entity
    )
