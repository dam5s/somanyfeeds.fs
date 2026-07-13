module DamoIoServer.FeedsProcessor

open System.Threading
open System.Threading.Tasks
open FSharp.Control
open Microsoft.Extensions.Logging

open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Xml
open DamoIoServer.Article
open DamoIoServer.SourcesRepository

type FeedsProcessor(logger: ILogger<FeedsProcessor>) =

    let articleToRecord (sourceFeed: SourceFeed) (article: Article) : ArticleRecord =
        { Title = Article.title article
          Link = Article.link article
          Content = Article.content article
          Media = Article.media article |> Option.map MediaRecord.ofMedia
          Date = Article.date article
          SourceType = sourceFeed.Type
          SourceName = sourceFeed.Name }

    let resultToList (sourceFeed: SourceFeed) (result: ProcessingResult) =
        List.map (articleToRecord sourceFeed) (Result.defaultValue [] result)

    let downloadAndProcessFeed
        (sourceFeed: SourceFeed)
        (cancellationToken: CancellationToken)
        : Task<ProcessingResult> =
        match sourceFeed.Feed with
        | Xml(url) ->
            task {
                let! download = DataGateway.download url

                cancellationToken.ThrowIfCancellationRequested()

                return
                    download
                    |> Result.bind Xml.processFeed
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

    member this.ProcessFeeds(sources: SourceFeed list, cancellationToken: CancellationToken) : TaskSeq<ArticleRecord> =
        taskSeq {
            for sourceFeed in sources do
                let! processingResult = downloadAndProcessFeed sourceFeed cancellationToken
                let articles = processingResult |> resultToList sourceFeed

                for a in articles do
                    yield a
        }
