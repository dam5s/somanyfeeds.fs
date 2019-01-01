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
            let feedUrls =
                listUrls()
                |> Result.map (List.map FeedUrl)
                |> Result.fold id (fun _ -> [])

            for url in feedUrls do
                yield url

            do! Async.Sleep(oneMinute * 10)
    }


let private logError (FeedUrl url) (msg : string) : string =
    eprintfn "There was an error while processing the feed with url %s: %s" url msg
    msg


let private logArticleError (url : string) (msg : string) : string =
    eprintfn "There was an error while persisting the article with url %s: %s" url msg
    msg


let private articleToRecord (FeedUrl feedUrl) (article : Article) : ArticleRecord =
    { Url = article.Link |> Option.orDefault (fun _ -> "")
      FeedUrl = feedUrl
      Content = article.Content
      Date = article.Date
    }


let private persistArticle (article : ArticleRecord) : Async<unit> =
    async {
        deleteArticle dataSource article.Url
        |> Result.bind (fun _ -> createArticle dataSource article)
        |> Result.mapError (logArticleError article.Url)
        |> ignore
    }


let private processFeed (feedUrl : FeedUrl) : ArticleRecord seq =
    printfn "Processing feed %A" feedUrl
    let articles = downloadFeed feedUrl
                   |> Result.bind Xml.processXmlFeed
                   |> Result.mapError (logError feedUrl)
                   |> Result.fold id (fun _ -> [])

    articles
    |> List.toSeq
    |> Seq.map (articleToRecord feedUrl)


let backgroundProcessing : Async<unit> =
    sequence
    |> AsyncSeq.map processFeed
    |> AsyncSeq.concatSeq
    |> AsyncSeq.iterAsyncParallel persistArticle
