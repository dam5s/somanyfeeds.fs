module ``ArticlesDataGateway tests``

open FsUnit
open System
open NUnit.Framework
open Time
open SoManyFeedsServer
open SoManyFeedsServer.ArticlesDataGateway


[<Test>]
let ``creating an article`` () =
    setTestDbConnectionString ()
    executeSql "delete from articles"

    let fields: ArticleFields =
      { Url = "http://example.com/my/articles/1"
        Title = "My Article"
        FeedUrl = "http://example.com/my/feed"
        Content = "This my article v1"
        Date = Some (Posix.fromDateTimeOffset DateTimeOffset.UtcNow)
      }

    let result = fields
                 |> ArticlesDataGateway.createOrUpdateArticle
                 |> Async.RunSynchronously

    match result with
    | Error _ ->
        failwith "Expected to get success back"

    | Ok created ->
        created.Url |> should equal fields.Url
        created.Title |> should equal fields.Title
        created.FeedUrl |> should equal fields.FeedUrl
        created.Content |> should equal fields.Content
        created.Date |> should equal fields.Date

        let persisted =
            queryDataContext (fun ctx ->
                query { for a in ctx.Public.Articles do
                        where (a.Id = created.Id)
                })
            |> Seq.head
            |> ArticlesDataGateway.entityToRecord

        persisted |> should equal created

