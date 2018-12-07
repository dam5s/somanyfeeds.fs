module DamoIOServer.FeedsProcessor

open DamoIOServer.Articles.Data
open DamoIOServer.Sources
open FeedsProcessing.Article
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Rss
open FeedsProcessing.Atom
open FeedsProcessing.DataGateway
open FeedsProcessing.Twitter


let private articleToRecord (sourceType : SourceType) (article : Article) : Record =
    { Title = article.Title
      Link = article.Link
      Content = article.Content
      Date = article.Date
      Source = sourceType
    }


let private orElse other =
    Result.fold id (fun _ -> other)


let private resultToList (sourceType : SourceType) (result : ProcessingResult) : Record list =
    List.map (articleToRecord sourceType) (orElse [] result)


let private processFeed (feed : Feed) : ProcessingResult =
    match feed with
    | Rss (source, url) -> Result.bind processRssFeed (downloadFeed url)
    | Atom (source, url) -> Result.bind processAtomFeed (downloadFeed url)
    | Twitter (handle) -> Result.bind (processTweets handle) (downloadTwitterTimeline handle)


let processFeeds (sources : Source list) : Record list =
    List.collect
        (fun (sourceType, feed) -> processFeed feed |> resultToList sourceType)
        sources
