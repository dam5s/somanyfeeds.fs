module SoManyFeedsServer.FeedsProcessor

open SoManyFeedsServer.FeedsPersistence
open DataAccess
open FSharp.Control
open FeedsProcessing
open FeedsProcessing.Article
open FeedsProcessing.DataGateway
open FeedsProcessing.Feeds


let private sequence : AsyncSeq<FeedRecord> =
    asyncSeq {
        let oneMinute = 1000 * 60
        let listAllFeeds = FeedsPersistence.listAllFeeds dataSource

        while true do
            let feeds =
                match listAllFeeds () with
                | Ok feeds -> feeds
                | Error _ -> []

            for feed in feeds do
                yield feed

            do! Async.Sleep (oneMinute / 2)
    }


let private persistArticles (articles : Article list) : Result<unit, string> =
    Error "not implemented"


let private logError (feed : FeedRecord) (msg : string) : unit =
    eprintfn "There was an error while processing the feed with url %s: %s" feed.Url msg


let private processFeed (feed : FeedRecord) : Async<unit> =
    async {
        downloadFeed (FeedUrl feed.Url)
            |> Result.bind Xml.processXmlFeed
            |> Result.bind persistArticles
            |> Result.mapError (logError feed)
            |> ignore
    }


let backgroundProcessing : Async<unit> =
    AsyncSeq.iterAsyncParallel
        processFeed
        sequence
