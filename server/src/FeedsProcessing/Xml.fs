module Server.FeedsProcessing.Xml

open System.Xml
open System
open Server.FeedsProcessing.Download

let getChildXpathValue (nsManager : XmlNamespaceManager) (node : XmlNode) (xpath : string) : string option =
    try
        let value = node.SelectSingleNode(xpath, nsManager).Value

        match value with
        | null -> None
        | _ -> Some value
    with
    | ex ->
        printfn "Error while reading xpath %s %s" xpath (ex.ToString())
        None


let parseDate (text : string) : DateTime option =
    try Some (DateTime.Parse text)
    with
    | ex ->
        printfn "Error while  parsing date %s %s" text (ex.ToString())
        None


let parse (downloaded : DownloadedFeed) : Result<XmlDocument, string> =
    try
        let doc = new XmlDocument()
        doc.LoadXml (downloadedString downloaded)
        Result.Ok doc
    with
    | ex ->
        Result.Error <| String.Format("There was an error parsing the XML. {0}", ex.ToString())
