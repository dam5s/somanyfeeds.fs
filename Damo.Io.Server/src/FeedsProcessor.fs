module DamoIoServer.FeedsProcessor

open DamoIoServer.ArticlesDataGateway
open DamoIoServer.Sources
open FSharp.Control
open FeedsProcessing
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Xml


let private articleToRecord sourceType article =
    { Title = Article.title article
      Link = Article.link article
      Content = Article.content article
      Date = Article.date article
      Source = sourceType
    }


let private resultToList sourceType (result: ProcessingResult) =
    List.map (articleToRecord sourceType) (Result.defaultValue [] result)

let private downloadAndProcessFeed feed: Async<ProcessingResult> =
    match feed with
    | Xml(_, url) ->
        async {
            let! download = downloadContent url
            return download |> Result.bind processFeed
        }


let processFeeds (sources: Source list): AsyncSeq<ArticleRecord> =
    asyncSeq {
        for (sourceType, feed) in sources do
            let! processingResult = downloadAndProcessFeed feed
            let articles = processingResult |> resultToList sourceType

            for a in articles do yield a
    }
