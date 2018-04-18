module Server.FeedsProcessing.Rss

open System.Xml
open System
open Server.FeedsProcessing.ProcessingResult
open Server.FeedsProcessing.Download
open Server.Articles.Data
open Server.SourceType

let private getChildText (nsManager : XmlNamespaceManager) (node : XmlNode) (name : string) : string option =
    let xpath = String.Format("descendant::{0}/text()", name)
    Xml.getChildXpathValue nsManager node xpath


let private buildRecord (nsManager : XmlNamespaceManager) (source : SourceType) (node : XmlNode) : Record =
    { Title = getChildText nsManager node "title"
      Link = getChildText nsManager node "link"
      Content = getChildText nsManager node "content:encoded"
                |> Option.orElse (getChildText nsManager node "description")
                |> Option.defaultValue ""
      Date = getChildText nsManager node "pubDate"
             |> Option.bind Xml.parseDate
      Source = source }


let private namespaceManager (doc : XmlDocument) =
    let nsManager = new XmlNamespaceManager(doc.NameTable)
    nsManager.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/")
    nsManager


let private parseArticles (source : SourceType) (doc : XmlDocument) : Record list =
    let nsManager = namespaceManager doc

    doc.SelectNodes("//item", nsManager)
        |> Seq.cast<XmlNode>
        |> Seq.map (buildRecord nsManager source)
        |> Seq.toList


let processRssFeed (source : SourceType) (downloadedFeed : DownloadedFeed) : ProcessingResult =
    Result.map (parseArticles source) (Xml.parse downloadedFeed)
