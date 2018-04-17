module Server.Processors.Rss

open Server.ArticlesProcessing
open Server.ArticlesData
open Server.Feeds
open System.Xml
open System

let private getChildText (nsManager : XmlNamespaceManager) (node : XmlNode) (name : string) : string option =
    try
        let xpath = String.Format("descendant::{0}/text()", name)
        let value = node.SelectSingleNode(xpath, nsManager).Value

        match value with
        | null -> None
        | _ -> Some value
    with
    | ex ->
        printfn "Error while reading xpath %s %s" name (ex.ToString())
        None

let private parseDate (text : string) : DateTime option =
    try Some (DateTime.Parse text)
    with
    | ex ->
        printfn "Error while  parsing date %s %s" text (ex.ToString())
        None


let private buildRecord (nsManager : XmlNamespaceManager) (feed : Feed) (node : XmlNode) : Record =
    { Title = getChildText nsManager node "title"
      Link = getChildText nsManager node "link"
      Content = getChildText nsManager node "content:encoded"
                |> Option.orElse (getChildText nsManager node "description")
                |> Option.defaultValue ""
      Date = getChildText nsManager node "pubDate"
             |> Option.bind parseDate
      Source = feed.Slug }


let private parseXml xml =
    try
        let doc = new XmlDocument()
        doc.LoadXml xml
        Result.Ok doc
    with
    | ex -> Result.Error <| String.Format("There was an error parsing the XML. {0}", ex.ToString())


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


let private fetchAndProcessRssFeed downloadFunction (feed : Feed) : ProcessingResult =
    downloadFunction feed.Info
        |> Result.bind parseXml
        |> Result.map (parseArticles feed)


let processFeed (downloadFunction : string -> Result<string, string>) (feed : Feed) : ProcessingResult =
    match feed.Type with
    | Rss -> fetchAndProcessRssFeed downloadFunction feed
    | _  -> ProcessingResult.Ok []
