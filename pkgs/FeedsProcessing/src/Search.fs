module FeedsProcessing.Search

open FSharp.Data

open FeedsProcessing.Download
open FeedsProcessing.Xml

let private tryParseHtml text =
    Try.value "tryParseHtml" (fun _ -> HtmlDocument.Parse text)

let private attrValue name (node: HtmlNode) =
    try
        Some(node.Attribute(name).Value())
    with _ ->
        None

let private isFeedLink (node: HtmlNode) =
    let rel = attrValue "rel" node |> Option.defaultValue ""
    let type_ = attrValue "type" node |> Option.defaultValue ""

    (String.contains "alternate" rel) && (String.contains "xml" type_)

let private hrefAsUrl (node: HtmlNode) =
    node |> attrValue "href" |> Option.map Url

let private findFeedUrlsInHtml (html: HtmlDocument) =
    html.CssSelect("link") |> Seq.filter isFeedLink |> Seq.choose hrefAsUrl

let private findFeedUrlsOnPage (download: Download) : Url seq =
    tryParseHtml download.Content
    |> Result.map findFeedUrlsInHtml
    |> Result.defaultValue (seq [])

type SearchResult =
    | FeedMatch of FeedMetadata
    | WebPageMatch of Url seq

let search (download: Download) : SearchResult =
    match Xml.tryGetMetadata download with
    | Some m -> FeedMatch m
    | None -> WebPageMatch(findFeedUrlsOnPage download)
