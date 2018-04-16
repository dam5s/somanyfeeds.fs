module Server.Processors.Atom

open Server.ArticlesProcessing
open Server.ArticlesData
open Server.Feeds
open System.Xml
open System


let private getChildXpathValue (nsManager : XmlNamespaceManager) (node : XmlNode) (xpath : string) : string option =
    try
        let value = node.SelectSingleNode(xpath, nsManager).Value

        match value with
        | null -> None
        | _ -> Some value
    with
    | ex ->
        printfn "Error while reading xpath %s %s" xpath (ex.ToString())
        None


let private getChildAttribute (nsManager : XmlNamespaceManager) (node : XmlNode) (nodeName : string) (attrName : string) : string option =
    let xpath = String.Format("descendant::atom:{0}/@{1}", nodeName, attrName)
    getChildXpathValue nsManager node xpath

let private getChildText (nsManager : XmlNamespaceManager) (node : XmlNode) (name : string) : string option =
    let xpath = String.Format("descendant::atom:{0}/text()", name)
    getChildXpathValue nsManager node xpath


let private parseDate (text : string) : DateTime option =
    try Some (DateTime.Parse text)
    with
    | ex ->
        printfn "Error while  parsing date %s %s" text (ex.ToString())
        None


let private buildRecord (nsManager : XmlNamespaceManager) (feed : Feed) (node : XmlNode) : Record =
    { Title = getChildText nsManager node "title"
      Link = getChildAttribute nsManager node "link" "href"
      Content = getChildText nsManager node "content"
                |> Option.defaultValue ""
      Date = getChildText nsManager node "published"
             |> Option.bind parseDate
      Source = feed.Slug }


let private fetchAndProcessAtomFeed downloadFunction feed : ProcessingResult =
    try
        let atomXml = downloadFunction feed.Info
        let doc = new XmlDocument()
        doc.LoadXml atomXml

        let nsManager = new XmlNamespaceManager(doc.NameTable)
        nsManager.AddNamespace("atom", "http://www.w3.org/2005/Atom")

        doc.SelectNodes("//atom:entry", nsManager)
            |> Seq.cast<XmlNode>
            |> Seq.map (buildRecord nsManager feed)
            |> Seq.toList
            |> ProcessingResult.Ok
    with
    | ex ->
        printfn "Error while processing feed %s" (ex.ToString())
        ProcessingResult.Error (ex.ToString())


let processFeed (downloadFunction : string -> string) (feed : Feed) : ProcessingResult =
    match feed.Type with
    | Atom -> fetchAndProcessAtomFeed downloadFunction feed
    | _  -> ProcessingResult.Ok []
