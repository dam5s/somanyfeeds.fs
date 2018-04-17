module Server.FeedsProcessing.Atom

open Server.FeedsProcessing
open Server.FeedsProcessing.Download
open Server.ArticlesData
open Server.Feeds
open System.Xml
open System


let private getChildAttribute (nsManager : XmlNamespaceManager) (node : XmlNode) (nodeName : string) (attrName : string) : string option =
    let xpath = String.Format("descendant::atom:{0}/@{1}", nodeName, attrName)
    Xml.getChildXpathValue nsManager node xpath


let private getChildText (nsManager : XmlNamespaceManager) (node : XmlNode) (name : string) : string option =
    let xpath = String.Format("descendant::atom:{0}/text()", name)
    Xml.getChildXpathValue nsManager node xpath


let private buildRecord (nsManager : XmlNamespaceManager) (feed : Feed) (node : XmlNode) : Record =
    { Title = getChildText nsManager node "title"
      Link = getChildAttribute nsManager node "link" "href"
      Content = getChildText nsManager node "content"
        |> Option.defaultValue ""
      Date = getChildText nsManager node "published"
        |> Option.bind Xml.parseDate
      Source = feed.Slug }


let private namespaceManager (doc : XmlDocument) =
    let nsManager = new XmlNamespaceManager(doc.NameTable)
    nsManager.AddNamespace("atom", "http://www.w3.org/2005/Atom")
    nsManager


let private parseArticles (feed : Feed) (doc : XmlDocument) : Record list =
    let nsManager = namespaceManager doc

    doc.SelectNodes("//atom:entry", nsManager)
        |> Seq.cast<XmlNode>
        |> Seq.map (buildRecord nsManager feed)
        |> Seq.toList


let processFeed (feed : Feed) (downloadedFeed : DownloadedFeed) : ProcessingResult =
    match feed.Type with
    | Atom -> Result.map (parseArticles feed) (Xml.parse downloadedFeed)
    | _  -> ProcessingResult.Ok []
