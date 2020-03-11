module DamoIOServer.FeedsProcessor

open DamoIOServer.ArticlesDataGateway
open DamoIOServer.Sources
open FeedsProcessing
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Twitter
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

let private consumerKey =
    Env.requireVar "TWITTER_CONSUMER_API_KEY"

let private consumerSecret =
    Env.requireVar "TWITTER_CONSUMER_SECRET"


let private processFeed feed: ProcessingResult =
    match feed with
    | Xml(_, url) -> Result.bind processXmlFeed (downloadFeed url)
    | Twitter(handle) -> Result.bind (processTweets handle) (downloadTwitterTimeline consumerKey consumerSecret handle)


let processFeeds (sources: Source list) =
    List.collect
        (fun (sourceType, feed) -> processFeed feed |> resultToList sourceType)
        sources
