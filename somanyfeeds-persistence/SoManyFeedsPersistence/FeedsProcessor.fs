module SoManyFeedsPersistence.FeedsProcessor

open FSharp.Control
open FeedJobsDataGateway
open FeedsProcessing
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Download
open SoManyFeedsPersistence.ArticlesDataGateway


type private Logs = Logs
let private logger = Logger<Logs>()
let private createMissing _ = FeedJobsDataGateway.createMissing
let private startJobs _ = FeedJobsDataGateway.startSome 5

let private sequence: AsyncSeq<Url> =
    asyncSeq {
        let! _ = createMissing()

        let! feedUrls =
            startJobs()
            |> AsyncResult.defaultValue Seq.empty

        for url in feedUrls do
            yield url
    }

let private everyFiveMinutes (s: AsyncSeq<'a>) =
    asyncSeq {
        let oneMinute = 1000 * 60

        while true do
            for element in s do
                yield element

            do! Async.Sleep(oneMinute * 5)
    }

let private logFeedError (Url url) err =
    err
    |> Explanation.wrapMessage (sprintf "There was an error while processing the feed with url %s: %s" url)
    |> logger.Error

let private logArticleError url err =
    err
    |> Explanation.wrapMessage (sprintf "There was an error while persisting the article with url %s: %s" url)
    |> logger.Error

let private completeJob feedUrl =
    feedUrl
    |> FeedJobsDataGateway.complete
    |> Async.RunSynchronously
    |> ignore

let private failJob feedUrl message =
    message
    |> logFeedError feedUrl
    |> JobFailure
    |> FeedJobsDataGateway.fail feedUrl
    |> Async.RunSynchronously
    |> ignore

let private articleToFields (Url feedUrl) article =
    { Url = (Article.link article) |> Option.defaultValue ""
      Title = (Article.title article) |> Option.defaultValue ""
      FeedUrl = feedUrl
      Content = (Article.content article)
      Date = (Article.date article)
    }

let private persistArticle fields =
    async {
        let! result = createOrUpdateArticle fields
                      |> AsyncResult.mapError (logArticleError fields.Url)

        result |> ignore
    }

let private processFeed feedUrl =
    asyncSeq {
        sprintf "Processing feed %A" feedUrl
        |> logger.Info
        |> ignore

        let! download = downloadContent feedUrl
        let processResult = download |> Result.bind Xml.processFeed

        match processResult with
        | Error explanation ->
            failJob feedUrl explanation
        | Ok articles ->
            completeJob feedUrl

            for a in articles do
                yield articleToFields feedUrl a
    }

let private backgroundProcessing (sequenceModifier: AsyncSeq<Url> -> AsyncSeq<Url>) =
    sequence
    |> sequenceModifier
    |> AsyncSeq.collect processFeed
    |> AsyncSeq.iterAsync persistArticle

let backgroundProcessingOnce =
    backgroundProcessing id

let backgroundProcessingInfinite =
    backgroundProcessing everyFiveMinutes
