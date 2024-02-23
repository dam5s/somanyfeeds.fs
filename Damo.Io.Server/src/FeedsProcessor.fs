module DamoIoServer.FeedsProcessor

open DamoIoServer.Article
open DamoIoServer.SourcesRepository
open FSharp.Control
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Xml
open Microsoft.Extensions.Logging

let private articleToRecord (sourceFeed: SourceFeed) (article: Article) : ArticleRecord =
    { Title = Article.title article
      Link = Article.link article
      Content = Article.content article
      Media = Article.media article |> Option.map MediaRecord.ofMedia
      Date = Article.date article
      SourceType = sourceFeed.Type
      SourceName = sourceFeed.Name }

let private resultToList (sourceFeed: SourceFeed) (result: ProcessingResult) =
    List.map (articleToRecord sourceFeed) (Result.defaultValue [] result)

let private downloadAndProcessFeed (logger: ILogger) (sourceFeed: SourceFeed) : Async<ProcessingResult> =
    match sourceFeed.Feed with
    | Xml(url) ->
        async {
            let! download = downloadContent url

            return
                download
                |> Result.bind processFeed
                |> Result.onOk (fun articles ->
                    let count = List.length articles
                    logger.LogInformation($"Parsed feed %A{url}, found %d{count} article(s)")
                )
                |> Result.onError (fun explanation ->
                    logger.LogError($"Error processing feed %A{url}")

                    for ex in explanation.Exceptions do
                        logger.LogError(ex, ex.Message)
                )
        }

let processFeeds (logger: ILogger) (sources: SourcesRepository.SourceFeed list) : AsyncSeq<ArticleRecord> =
    asyncSeq {
        for sourceFeed in sources do
            let! processingResult = downloadAndProcessFeed logger sourceFeed
            let articles = processingResult |> resultToList sourceFeed

            for a in articles do
                yield a
    }
