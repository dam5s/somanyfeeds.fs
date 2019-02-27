module SoManyFeedsServer.FeedsProcessor

open DataAccess
open FSharp.Control
open FeedJobsDataGateway
open FeedsProcessing
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open SoManyFeedsServer.ArticlesDataGateway


let private createMissing _ = FeedJobsDataGateway.createMissing dataSource
let private startJobs _ = FeedJobsDataGateway.startSome dataSource 5


let private sequence : AsyncSeq<FeedUrl> =
    asyncSeq {
        let oneMinute = 1000 * 60

        while true do
            let! _ = createMissing ()

            let! feedUrls =
                startJobs ()
                |> AsyncResult.defaultValue []

            for url in feedUrls do
                yield url

            do! Async.Sleep (oneMinute * 5)
    }


let private logger = getLogger "somanyfeeds-server.feeds-processor"


let private logFeedError (FeedUrl url) (msg : string) : string =
    sprintf "There was an error while processing the feed with url %s: %s" url msg
    |> logError logger
    |> always msg


let private logArticleError (url : string) (msg : string) : string =
    sprintf "There was an error while persisting the article with url %s: %s" url msg
    |> logError logger
    |> always msg


let private completeJob (feedUrl : FeedUrl) =
    feedUrl
    |> FeedJobsDataGateway.complete dataSource
    |> Async.RunSynchronously
    |> ignore


let private failJob (feedUrl : FeedUrl) (message : string) =
    message
    |> logFeedError feedUrl
    |> JobFailure
    |> FeedJobsDataGateway.fail dataSource feedUrl
    |> Async.RunSynchronously
    |> ignore


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
    sprintf "Processing feed %A" feedUrl
    |> logInfo logger
    |> ignore

    let processResult =
        feedUrl
        |> downloadFeed
        |> Result.bind Xml.processXmlFeed

    match processResult with
    | Ok articles ->
        completeJob feedUrl

        articles
        |> List.toSeq
        |> Seq.map (articleToFields feedUrl)

    | Error message ->
        failJob feedUrl message

        Seq.empty


let backgroundProcessing : Async<unit> =
    sequence
    |> AsyncSeq.map processFeed
    |> AsyncSeq.concatSeq
    |> AsyncSeq.iterAsyncParallel persistArticle
