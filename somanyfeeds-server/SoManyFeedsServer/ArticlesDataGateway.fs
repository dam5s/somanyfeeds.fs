module SoManyFeedsServer.ArticlesDataGateway

open System
open System.Data.Common
open SoManyFeedsServer.DataSource
open AsyncResult.Operators


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


let createOrUpdateArticle (dataSource : DataSource) (fields : ArticleFields) : AsyncResult<ArticleRecord> =
    let bindings =
        [
        Binding ("@Url", fields.Url)
        Binding ("@Title", fields.Title)
        Binding ("@FeedUrl", fields.FeedUrl)
        Binding ("@Content", fields.Content)
        optionBinding ("@Date", fields.Date)
        ]

    let mapping =
        fun (record : DbDataRecord) ->
            { Id = record.GetInt64(0)
              Url = fields.Url
              Title = fields.Title
              FeedUrl = fields.FeedUrl
              Content = fields.Content
              Date = fields.Date
            }

    query dataSource
        """ insert into articles (url, title, feed_url, content, date)
            values (@Url, @Title, @FeedUrl, @Content, @Date)
            on conflict (url, feed_url) do update set
                title = excluded.title,
                content = excluded.content,
                date = excluded.date
            returning id
        """
        bindings
        mapping
        <!> (List.first >> Option.get)
