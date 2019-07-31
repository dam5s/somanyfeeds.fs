module DamoIOServer.FeedsProcessor

open DamoIOServer.ArticlesDataGateway
open DamoIOServer.Sources
open FeedsProcessing
open FeedsProcessing.Article
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Xml
open FeedsProcessing.DataGateway
open FeedsProcessing.Twitter


let private articleToRecord (sourceType : SourceType) (article : Article) : ArticleRecord =
    { Title = Article.title article
      Link = Article.link article
      Content = Article.content article
      Date = Article.date article
      Source = sourceType
    }


let private resultToList (sourceType : SourceType) (result : ProcessingResult) : ArticleRecord list =
    List.map (articleToRecord sourceType) (Result.defaultValue [] result)

let private consumerKey =
    Env.varRequired "TWITTER_CONSUMER_API_KEY"

let private consumerSecret =
    Env.varRequired "TWITTER_CONSUMER_SECRET"


let private processFeed (feed : Feed) : ProcessingResult =
    match feed with
    | Xml (_, url) -> Result.bind processXmlFeed (downloadFeed url)
    | Twitter (handle) -> Result.bind (processTweets handle) (downloadTwitterTimeline consumerKey consumerSecret handle)


let processFeeds (sources : Source list) : ArticleRecord list =
    List.collect
        (fun (sourceType, feed) -> processFeed feed |> resultToList sourceType)
        sources
