module Damo.Io.Server.FeedsProcessor

open FSharp.Control
open FeedsProcessing.DataGateway
open FeedsProcessing.XmlFeedDecoder
open Microsoft.Extensions.Logging
open System.Threading

open FeedsPersistence.ArticleRecord
open FeedsPersistence.FeedsRepository
open FeedsProcessing.Article
open FeedsProcessing.Feeds

type FeedsProcessor(logger: ILogger<FeedsProcessor>, dataGateway: DataGateway, xmlDecoder: XmlFeedDecoder) =

    let articleToRecord (feed: FeedRecord) (article: Article) : ArticleRecord =
        { Title = Article.title article
          Link = Article.link article
          Content = Article.content article
          Media = Article.media article |> Option.map MediaRecord.ofMedia
          Date = Article.date article
          FeedName = feed.Name }

    let articlesToList (sourceFeed: FeedRecord) (articles: Article list) =
        articles |> List.map (articleToRecord sourceFeed)

    let downloadAndProcessFeed (feed: FeedRecord) (cancellationToken: CancellationToken) : TaskResult<Article list> =
        match feed.Feed with
        | Xml(url) ->
            taskResult {
                let! download = dataGateway.DownloadAsync url

                cancellationToken.ThrowIfCancellationRequested()

                let! articles = xmlDecoder.TryDecodeAsync download

                let count = List.length articles
                logger.LogInformation($"Parsed feed %A{url}, found %d{count} article(s)")

                return articles
            }

    member _.ProcessFeeds(feeds: FeedRecord list, cancellationToken: CancellationToken) : TaskSeq<ArticleRecord> =
        taskSeq {
            for feed in feeds do
                let! processingResult = downloadAndProcessFeed feed cancellationToken
                let articles = processingResult |> Result.defaultValue [] |> articlesToList feed

                for a in articles do
                    yield a
        }
