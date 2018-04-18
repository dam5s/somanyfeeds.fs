module Server.FeedsProcessor

open System
open FSharp.Data
open Server.Articles.Data
open Server.Feeds
open Server.FeedUrl
open Server.FeedsProcessing.ProcessingResult
open Server.FeedsProcessing.Download
open Server.FeedsProcessing.Rss
open Server.FeedsProcessing.Atom


let private downloadFeed (url : FeedUrl) : DownloadResult =
    try
        feedUrlString url
            |> Http.RequestString
            |> DownloadedFeed
            |> Result.Ok
    with
    | ex -> Result.Error <| String.Format("There was an error downloading the feed. {0}", ex.ToString())


let private resultToList (result : ProcessingResult) : Record list =
    match result with
    | Ok records -> records
    | Error _ -> []


let private processFeed (feed : Feed) : ProcessingResult =
    match feed with
    | Rss (source, url) -> Result.bind (processRssFeed source) (downloadFeed url)
    | Atom (source, url) -> Result.bind (processAtomFeed source) (downloadFeed url)
    | Twitter (_) -> Error "Twitter not supported yet"



let processFeeds
    (updateAll : Record list -> unit)
    (findAllFeeds : unit -> Feed list) =

    let newRecords =
        (findAllFeeds())
            |> List.map processFeed
            |> List.collect resultToList

    updateAll newRecords
