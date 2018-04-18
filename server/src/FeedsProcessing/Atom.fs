module Server.FeedsProcessing.Atom

open System.Xml
open System
open Server.FeedsProcessing.ProcessingResult
open Server.FeedsProcessing.Download
open Server.ArticlesData
open Server.SourceType


let private getChildAttribute (nsManager : XmlNamespaceManager) (node : XmlNode) (nodeName : string) (attrName : string) : string option =
    let xpath = String.Format("descendant::atom:{0}/@{1}", nodeName, attrName)
    Xml.getChildXpathValue nsManager node xpath


let private getChildText (nsManager : XmlNamespaceManager) (node : XmlNode) (name : string) : string option =
    let xpath = String.Format("descendant::atom:{0}/text()", name)
    Xml.getChildXpathValue nsManager node xpath


let private buildRecord (nsManager : XmlNamespaceManager) (source : SourceType) (node : XmlNode) : Record =
    { Title = getChildText nsManager node "title"
      Link = getChildAttribute nsManager node "link" "href"
      Content = getChildText nsManager node "content"
        |> Option.defaultValue ""
      Date = getChildText nsManager node "published"
        |> Option.bind Xml.parseDate
      Source = source }


let private namespaceManager (doc : XmlDocument) =
    let nsManager = new XmlNamespaceManager(doc.NameTable)
    nsManager.AddNamespace("atom", "http://www.w3.org/2005/Atom")
    nsManager


let private parseArticles (source : SourceType) (doc : XmlDocument) : Record list =
    let nsManager = namespaceManager doc

    doc.SelectNodes("//atom:entry", nsManager)
        |> Seq.cast<XmlNode>
        |> Seq.map (buildRecord nsManager source)
        |> Seq.toList


let processAtomFeed (source : SourceType) (downloadedFeed : DownloadedFeed) : ProcessingResult =
    Result.map (parseArticles source) (Xml.parse downloadedFeed)
