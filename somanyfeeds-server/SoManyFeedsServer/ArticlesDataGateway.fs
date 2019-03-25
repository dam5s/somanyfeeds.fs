module SoManyFeedsServer.ArticlesDataGateway

open System
open System.Data.Common
open SoManyFeedsServer.DataSource
open FSharp.Data.Sql.Common


type ArticleRecord =
    { Id : int64
      Url : string
      Title : string
      FeedUrl : string
      Content : string
      Date : DateTimeOffset option
    }


type ArticleFields =
    { Url : string
      Title : string
      FeedUrl : string
      Content : string
      Date : DateTimeOffset option
    }


let mapArticle (record : DbDataRecord) : ArticleRecord =
    let date index =
        match Convert.IsDBNull index with
        | true -> None
        | false -> Some <| new DateTimeOffset (record.GetDateTime index)

    { Id = record.GetInt64 0
      Url = record.GetString 1
      Title = record.GetString 2
      FeedUrl = record.GetString 3
      Content = record.GetString 4
      Date = date 5
    }


let entityToRecord (entity : ArticleEntity) : ArticleRecord =
    { Id = entity.Id
      Url = entity.Url
      Title = entity.Title
      FeedUrl = entity.FeedUrl
      Content = entity.Content
      Date = entity.Date |> Option.map (fun d -> new DateTimeOffset(d))
    }


let createOrUpdateArticle (fields : ArticleFields) : AsyncResult<ArticleRecord> =
    dataAccessOperation (fun ctx ->
        let entity = ctx.Public.Articles.Create ()
        entity.Url <- fields.Url
        entity.Title <- fields.Title
        entity.FeedUrl <- fields.FeedUrl
        entity.Content <- fields.Content
        entity.Date <- fields.Date |> Option.map (fun d -> d.UtcDateTime)
        entity.OnConflict <- OnConflict.Update

        ctx.SubmitUpdates ()

        entityToRecord entity
    )
