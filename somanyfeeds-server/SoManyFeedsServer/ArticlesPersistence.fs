module SoManyFeedsServer.ArticlesPersistence

open System
open SoManyFeedsServer.DataSource


type ArticleRecord =
    { Url : string
      FeedUrl : string
      Content : string
      Date : DateTimeOffset option
    }


let createArticle (dataSource : DataSource) (record : ArticleRecord) : Result<ArticleRecord, string> =
    let bindings =
        [
        Binding("@Url", record.Url)
        Binding("@FeedUrl", record.FeedUrl)
        Binding("@Content", record.Content)
        optionBinding ("@Date", record.Date)
        ]

    update dataSource
        """ insert into articles (url, feed_url, content, date)
            values (@Url, @FeedUrl, @Content, @Date)
        """
        bindings
        |> Result.map (fun _ -> record)


let deleteArticle (dataSource : DataSource) (url : string) : Result<unit, string> =
    let bindings =
        [
        Binding("@Url", url)
        ]

    update dataSource
        "delete from articles where url = @Url"
        bindings
        |> Result.map (fun _ -> ())
