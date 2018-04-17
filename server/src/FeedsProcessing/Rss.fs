module Server.FeedsProcessing.Rss

open Server.FeedsProcessing
open Server.FeedsProcessing.Download
open Server.ArticlesData
open Server.Feeds
open System.Xml
open System

let private getChildText (nsManager : XmlNamespaceManager) (node : XmlNode) (name : string) : string option =
    let xpath = String.Format("descendant::{0}/text()", name)
    Xml.getChildXpathValue nsManager node xpath


let private buildRecord (nsManager : XmlNamespaceManager) (feed : Feed) (node : XmlNode) : Record =
    { Title = getChildText nsManager node "title"
      Link = getChildText nsManager node "link"
      Content = getChildText nsManager node "content:encoded"
                |> Option.orElse (getChildText nsManager node "description")
                |> Option.defaultValue ""
      Date = getChildText nsManager node "pubDate"
             |> Option.bind Xml.parseDate
      Source = feed.Slug }


let private namespaceManager (doc : XmlDocument) =
    let nsManager = new XmlNamespaceManager(doc.NameTable)
    nsManager.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/")
    nsManager


let private parseArticles (feed : Feed) (doc : XmlDocument) : Record list =
    let nsManager = namespaceManager doc

    doc.SelectNodes("//item", nsManager)
        |> Seq.cast<XmlNode>
        |> Seq.map (buildRecord nsManager feed)
        |> Seq.toList


let processFeed (feed : Feed) (downloadedFeed : DownloadedFeed) : ProcessingResult =
    match feed.Type with
    | Rss -> Result.map (parseArticles feed) (Xml.parse downloadedFeed)
    | _  -> ProcessingResult.Ok []
