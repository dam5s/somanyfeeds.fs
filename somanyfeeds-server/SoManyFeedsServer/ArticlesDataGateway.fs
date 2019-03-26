module SoManyFeedsServer.ArticlesDataGateway

open Time
open SoManyFeedsServer.DataSource
open FSharp.Data.Sql.Common


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
        let entity = ctx.Public.Articles.Create ()
        entity.Url <- fields.Url
        entity.Title <- fields.Title
        entity.FeedUrl <- fields.FeedUrl
        entity.Content <- fields.Content
        entity.Date <- fields.Date |> Option.map Posix.toDateTime
        entity.OnConflict <- OnConflict.Update

        ctx.SubmitUpdates ()

        entityToRecord entity
    )
