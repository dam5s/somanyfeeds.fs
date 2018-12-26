module FeedsProcessing.Rss

open System
open FSharp.Data

open FeedsProcessing.Article
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Download


type private RssProvider = XmlProvider<"../feeds-processing/Resources/samples/medium.rss.sample">


let private parse (xml : string) : Result<RssProvider.Rss, string> =
    try
        Ok <| RssProvider.Parse xml
    with
    | ex ->
        printfn "Could not parse RSS\n\n%s\n\nGot exception %s" xml (ex.ToString ())
        Error "Could not parse RSS"


let private itemToArticle (item : RssProvider.Item) : Article =
    { Title = Xml.stringToOption item.Title
      Link = Xml.stringToOption item.Link
      Content = Xml.stringToOption item.Encoded |> Option.defaultValue ""
      Date = Some item.PubDate
    }


let private rssToArticles (rss : RssProvider.Rss) : Article list =
    rss.Channel.Items
        |> Array.toList
        |> List.map itemToArticle


let processRssFeed (DownloadedFeed downloaded) : ProcessingResult =
    parse downloaded |> Result.map rssToArticles
