module FeedsProcessing.Xml

open System
open FSharp.Data
open FeedsProcessing.Article
open FeedsProcessing.Download
open FeedsProcessing.ProcessingResult


let private stringToOption text =
    if String.IsNullOrWhiteSpace text
    then None
    else Some text


let private tryOperation (operation : unit -> 'T) : Result<'T, string> =
    try Ok <| operation()
    with ex -> Error <| ex.Message.Trim()


let private bindOperation (operation : unit -> Result<'T, string>) : Result<'T, string> =
    try operation()
    with ex -> Error <| ex.Message.Trim()


module private Rss =

    type private RssProvider = XmlProvider<"../feeds-processing/Resources/samples/rss.sample">

    let private itemToArticle (item : RssProvider.Item) : Article =
        { Title = stringToOption item.Title
          Link = stringToOption item.Link
          Content = item.Encoded
                    |> Option.bind stringToOption
                    |> Option.orElse item.Description
                    |> Option.defaultValue ""
          Date = Some item.PubDate
        }

    let private toArticles (rss : RssProvider.Rss) : Result<Article list, string> =
        tryOperation (fun _ ->
            rss.Channel.Items
            |> Seq.map itemToArticle
            |> Seq.toList
        )

    let private parse (xml : string) : Result<RssProvider.Rss, string> =
        tryOperation (fun _ -> RssProvider.Parse xml)

    let processRss (DownloadedFeed downloaded) : ProcessingResult =
        parse downloaded |> Result.bind toArticles


module private Atom =

    type private AtomProvider = XmlProvider<"../feeds-processing/Resources/samples/github.atom.sample">

    let private entryToArticle (entry : AtomProvider.Entry) : Article =
        { Title = stringToOption entry.Title.Value
          Link = stringToOption entry.Link.Href
          Content = stringToOption entry.Content.Value |> Option.defaultValue ""
          Date = Some entry.Published
        }

    let private toArticles (atom : AtomProvider.Feed) : Result<Article list, string> =
        bindOperation (fun _ ->
            match atom.Entries with
            | [||] ->
                Error "Expected at least one atom entry"
            | entries ->
                entries
                |> Seq.map entryToArticle
                |> Seq.toList
                |> Ok
        )

    let private parse (xml : string) : Result<AtomProvider.Feed, string> =
        tryOperation (fun _ -> AtomProvider.Parse xml)

    let processAtom (DownloadedFeed downloaded) : ProcessingResult =
        parse downloaded |> Result.bind toArticles


module private Rdf =

    type private RdfProvider = XmlProvider<"../feeds-processing/Resources/samples/slashdot.rdf.sample">

    let private itemToArticle (item : RdfProvider.Item) : Article =
        { Title = stringToOption item.Title
          Link = stringToOption item.Link
          Content = stringToOption item.Description |> Option.defaultValue ""
          Date = Some item.Date
        }

    let private toArticles (rdf : RdfProvider.Rdf) : Result<Article list, string> =
        bindOperation (fun _ ->
            match rdf.Items with
            | [||] ->
                Error "Expected at least one rdf item"
            | items ->
                items
                |> Seq.map itemToArticle
                |> Seq.toList
                |> Ok
        )

    let private parse (xml : string) : Result<RdfProvider.Rdf, string> =
        tryOperation (fun _ -> RdfProvider.Parse xml)

    let processRdf (DownloadedFeed downloaded) : ProcessingResult =
        parse downloaded |> Result.bind toArticles


let private applyProcessor (downloaded : DownloadedFeed) (result : ProcessingResult) (processor : DownloadedFeed -> ProcessingResult) : ProcessingResult =
    match result with
    | Ok articles -> Ok articles
    | Error msg ->
        match processor downloaded with
        | Ok articles -> Ok articles
        | Error nextMsg -> Error(sprintf "%s, %s" msg nextMsg)


let processXmlFeed (downloaded : DownloadedFeed) : ProcessingResult =
    [
        Rss.processRss
        Atom.processAtom
        Rdf.processRdf
    ]
        |> List.fold (applyProcessor downloaded) (Error "")
        |> Result.mapError (sprintf "Failed all the parsers: %s")
