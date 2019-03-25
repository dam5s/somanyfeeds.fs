module SoManyFeedsServer.FeedsProcessor

open FSharp.Control
open FeedJobsDataGateway
open FeedsProcessing
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open SoManyFeedsServer.ArticlesDataGateway
open SoManyFeedsServer.DataSource


type private Logs = Logs
let private logger = createLogger<Logs>


let private createMissing _ = FeedJobsDataGateway.createMissing dataContext
let private startJobs _ = FeedJobsDataGateway.startSome dataContext 5


let private sequence : AsyncSeq<FeedUrl> =
    asyncSeq {
        let oneMinute = 1000 * 60

        while true do
            let! _ = createMissing ()

            let! feedUrls =
                startJobs ()
                |> AsyncResult.defaultValue Seq.empty

            for url in feedUrls do
                yield url

            do! Async.Sleep (oneMinute * 5)
    }


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
    |> FeedJobsDataGateway.complete dataContext
    |> Async.RunSynchronously
    |> ignore


let private failJob (feedUrl : FeedUrl) (message : string) =
    message
    |> logFeedError feedUrl
    |> JobFailure
    |> FeedJobsDataGateway.fail dataContext feedUrl
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
        let! result = createOrUpdateArticle dataContext fields
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
