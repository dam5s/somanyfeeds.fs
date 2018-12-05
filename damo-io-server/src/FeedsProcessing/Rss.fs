module Server.FeedsProcessing.Rss

open System
open FSharp.Data
open Server.FeedsProcessing.ProcessingResult
open Server.FeedsProcessing.Download
open Server.Articles.Data
open Server.SourceType


type private RssProvider = XmlProvider<"../damo-io-server/resources/samples/medium.rss.sample">


let private parse (xml : string) : Result<RssProvider.Rss, string> =
    try
        Ok <| RssProvider.Parse xml
    with
    | ex ->
        printfn "Could not parse RSS\n\n%s\n\nGot exception %s" xml (ex.ToString ())
        Error "Could not parse RSS"


let private itemToRecord (source : SourceType) (item : RssProvider.Item) : Record =
    { Title = Xml.stringToOption item.Title
      Link = Xml.stringToOption item.Link
      Content = Xml.stringToOption item.Encoded |> Option.defaultValue ""
      Date = Some item.PubDate
      Source = source
    }


let private rssToRecords (source : SourceType) (rss : RssProvider.Rss) : Record list =
    rss.Channel.Items
        |> Array.toList
        |> List.map (itemToRecord source)


let processRssFeed (source : SourceType) (downloaded : DownloadedFeed) : ProcessingResult =
    parse (downloadedString downloaded)
        |> Result.map (rssToRecords source)
