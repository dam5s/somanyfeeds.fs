module DamoIoServer.FeedsProcessor

open DamoIoServer.Article
open FSharp.Control
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Xml
open Microsoft.Extensions.Logging

let private articleToRecord sourceType (article: Article) : ArticleRecord =
    { Title = Article.title article
      Link = Article.link article
      Content = Article.content article
      Media = Article.media article |> Option.map MediaRecord.ofMedia
      Date = Article.date article
      Source = sourceType }

let private resultToList sourceType (result: ProcessingResult) =
    List.map (articleToRecord sourceType) (Result.defaultValue [] result)

let private downloadAndProcessFeed (logger: ILogger) feed : Async<ProcessingResult> =
    match feed with
    | Xml(_, url) ->
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
        for sourceType, feed in sources do
            let! processingResult = downloadAndProcessFeed logger feed
            let articles = processingResult |> resultToList sourceType

            for a in articles do
                yield a
    }
