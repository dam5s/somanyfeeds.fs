module ``ArticlesDataGateway tests``

open FsUnit
open NUnit.Framework
open SoManyFeedsPersistence
open SoManyFeedsPersistence.ArticlesDataGateway
open System
open Time


let private fieldsOf (record: ArticleRecord) =
    { Url = record.Url
      Title = record.Title
      FeedUrl = record.FeedUrl
      Content = record.Content
      Date = record.Date
    }

let private find id =
    queryDataContext (fun ctx ->
        query { for a in ctx.Public.Articles do
                where (a.Id = id)
        })
    |> Seq.head
    |> ArticlesDataGateway.entityToRecord


[<Test>]
let ``creating then updating an article``() =
    setTestDbConnectionString()
    executeAllSql
        [
        "delete from bookmarks"
        "delete from read_articles"
        "delete from articles"
        ]

    let fields: ArticleFields =
        { Url = "http://example.com/my/articles/1"
          Title = "My Article"
          FeedUrl = "http://example.com/my/feed"
          Content = "This my article v1"
          Date = Some(Posix.fromDateTimeOffset DateTimeOffset.UtcNow)
        }


    let createResult =
        fields
        |> ArticlesDataGateway.createOrUpdateArticle
        |> Async.RunSynchronously

    match createResult with
    | Error err -> failwithf "Expected Ok, but got Error '%s'" err.Message
    | Ok created ->
        fieldsOf created |> should equal fields
        find created.Id |> should equal created


    let updatedFields =
        { fields with
            Title = "My Article v2"
            Content = "This my article v2"
            Date = Some(Posix.fromDateTimeOffset DateTimeOffset.UtcNow)
        }

    let updateResult = updatedFields
                       |> ArticlesDataGateway.createOrUpdateArticle
                       |> Async.RunSynchronously

    match updateResult with
    | Error err -> failwithf "Expected Ok, but got Error '%s'" err.Message
    | Ok updated ->
        fieldsOf updated |> should equal updatedFields
        find updated.Id |> should equal updated
