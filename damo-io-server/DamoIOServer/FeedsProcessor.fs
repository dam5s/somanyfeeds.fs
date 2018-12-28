module DamoIOServer.FeedsProcessor

open DamoIOServer.ArticlesPersistence
open DamoIOServer.Sources
open FeedsProcessing.Article
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Xml
open FeedsProcessing.DataGateway
open FeedsProcessing.Twitter


let private articleToRecord (sourceType : SourceType) (article : Article) : ArticleRecord =
    { Title = article.Title
      Link = article.Link
      Content = article.Content
      Date = article.Date
      Source = sourceType
    }


let private orElse other =
    Result.fold id (fun _ -> other)


let private resultToList (sourceType : SourceType) (result : ProcessingResult) : ArticleRecord list =
    List.map (articleToRecord sourceType) (orElse [] result)


let private processFeed (feed : Feed) : ProcessingResult =
    match feed with
    | Xml (source, url) -> Result.bind processXmlFeed (downloadFeed url)
    | Twitter (handle) -> Result.bind (processTweets handle) (downloadTwitterTimeline handle)


let processFeeds (sources : Source list) : ArticleRecord list =
    List.collect
        (fun (sourceType, feed) -> processFeed feed |> resultToList sourceType)
        sources
