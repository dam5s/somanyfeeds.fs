module SoManyFeedsServer.ArticlesPersistence

open System
open System.Data.Common
open SoManyFeedsServer.DataSource


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


let private mapArticle (record : DbDataRecord) : ArticleRecord =
    let date index =
        match Convert.IsDBNull(index) with
        | true -> None
        | false -> Some <| new DateTimeOffset(record.GetDateTime(index))

    { Id = record.GetInt64(0)
      Url = record.GetString(1)
      Title = record.GetString(2)
      FeedUrl = record.GetString(3)
      Content = record.GetString(4)
      Date = date 5
    }


let createArticle (dataSource : DataSource) (fields : ArticleFields) : Result<ArticleRecord, string> =
    let bindings =
        [
        Binding("@Url", fields.Url)
        Binding("@Title", fields.Title)
        Binding("@FeedUrl", fields.FeedUrl)
        Binding("@Content", fields.Content)
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
            returning id
        """
        bindings
        mapping
        |> Result.map (List.first >> Option.get)


let deleteArticle (dataSource : DataSource) (url : string) (feedUrl : string) : Result<unit, string> =
    let bindings =
        [
        Binding("@Url", url)
        Binding("@FeedUrl", feedUrl)
        ]

    update dataSource
        "delete from articles where url = @Url and feed_url = @FeedUrl"
        bindings
        |> Result.map (fun _ -> ())


let listRecentArticles (dataSource : DataSource) (feedUrls : string list) : Result<ArticleRecord list, string> =
    match feedUrls with
    | [] -> Ok []
    | urls ->
        let urlArgs, bindings = inBindings "@FeedUrl" urls
        let sql =
            sprintf
                """ select id, url, title, feed_url, content, date
                    from articles
                    where feed_url in (%s)
                    order by date desc
                    limit 100
                """
                urlArgs

        query dataSource sql bindings mapArticle
