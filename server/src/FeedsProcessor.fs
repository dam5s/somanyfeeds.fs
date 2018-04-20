module Server.FeedsProcessor

open Server.Articles.Data
open Server.Feeds
open Server.FeedsProcessing.ProcessingResult
open Server.FeedsProcessing.Rss
open Server.FeedsProcessing.Atom
open Server.DataGateway
open Server.FeedsProcessing.Twitter


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
