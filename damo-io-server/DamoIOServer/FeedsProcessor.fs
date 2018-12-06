module DamoIOServer.FeedsProcessor

open DamoIOServer.Articles.Data
open DamoIOServer.Feeds
open DamoIOServer.FeedsProcessing.ProcessingResult
open DamoIOServer.FeedsProcessing.Rss
open DamoIOServer.FeedsProcessing.Atom
open DamoIOServer.DataGateway
open DamoIOServer.FeedsProcessing.Twitter


let private resultToList (result : ProcessingResult) : Record list =
    match result with
    | Ok records -> records
    | Error _ -> []


let private processFeed (feed : Feed) : ProcessingResult =
    match feed with
    | Rss (source, url) -> Result.bind (processRssFeed source) (downloadFeed url)
    | Atom (source, url) -> Result.bind (processAtomFeed source) (downloadFeed url)
    | Twitter (handle) -> Result.bind (processTweets handle) (downloadTwitterTimeline handle)


let processFeeds (feeds : Feed list) : Record list =
    feeds
        |> List.map processFeed
        |> List.collect resultToList
