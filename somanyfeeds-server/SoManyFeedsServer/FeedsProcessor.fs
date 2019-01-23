module SoManyFeedsServer.FeedsProcessor

open DataAccess
open FSharp.Control
open FeedsProcessing
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open SoManyFeedsServer.ArticlesPersistence


let private sequence : AsyncSeq<FeedUrl> =
    asyncSeq {
        let oneMinute = 1000 * 60
        let listUrls = FeedsPersistence.listUrls dataSource

        while true do
            let! feedUrls =
                listUrls ()
                |> AsyncResult.map (List.map FeedUrl)
                |> AsyncResult.defaultValue []

            for url in feedUrls do
                yield url

            do! Async.Sleep (oneMinute * 10)
    }


let private logError (FeedUrl url) (msg : string) : string =
    eprintfn "There was an error while processing the feed with url %s: %s" url msg
    msg


let private logArticleError (url : string) (msg : string) : string =
    eprintfn "There was an error while persisting the article with url %s: %s" url msg
    msg


let private articleToFields (FeedUrl feedUrl) (article : Article) : ArticleFields =
    { Url = article.Link |> Option.defaultValue ""
      Title = article.Title |> Option.defaultValue ""
      FeedUrl = feedUrl
      Content = article.Content
      Date = article.Date
    }


let private persistArticle (fields : ArticleFields) : Async<unit> =
    async {
        deleteArticle dataSource fields.Url fields.FeedUrl
        |> AsyncResult.bind (fun _ -> createArticle dataSource fields)
        |> AsyncResult.mapError (logArticleError fields.Url)
        |> ignore
    }


let private processFeed (feedUrl : FeedUrl) : ArticleFields seq =
    printfn "Processing feed %A" feedUrl
    let articles = downloadFeed feedUrl
                   |> Result.bind Xml.processXmlFeed
                   |> Result.mapError (logError feedUrl)
                   |> Result.defaultValue []

    articles
    |> List.toSeq
    |> Seq.map (articleToFields feedUrl)


let backgroundProcessing : Async<unit> =
    sequence
    |> AsyncSeq.map processFeed
    |> AsyncSeq.concatSeq
    |> AsyncSeq.iterAsyncParallel persistArticle
