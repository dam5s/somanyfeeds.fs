module SoManyFeedsServer.FeedsProcessor

open DataAccess
open FSharp.Control
open FeedsProcessing
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open SoManyFeedsServer.ArticlesDataGateway


let private sequence : AsyncSeq<FeedUrl> =
    asyncSeq {
        let oneMinute = 1000 * 60
        let listUrls = FeedsDataGateway.listUrls dataSource

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
    { Url = (Article.link article) |> Option.defaultValue ""
      Title = (Article.title article) |> Option.defaultValue ""
      FeedUrl = feedUrl
      Content = (Article.content article)
      Date = (Article.date article)
    }


let private persistArticle (fields : ArticleFields) : Async<unit> =
    async {
        let! result = createOrUpdateArticle dataSource fields
                      |> AsyncResult.mapError (logArticleError fields.Url)

        result |> ignore
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
