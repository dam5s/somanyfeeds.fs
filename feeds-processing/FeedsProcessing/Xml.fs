module FeedsProcessing.Xml

open FSharp.Data
open FeedsProcessing.Article
open FeedsProcessing.Download
open FeedsProcessing.ProcessingResult
open System


let private stringToOption text =
    if String.IsNullOrWhiteSpace text
    then None
    else Some text


module private Rss =

    type private RssProvider = XmlProvider<"../feeds-processing/Resources/samples/rss.sample">

    let private itemToArticle (item: RssProvider.Item) =
        Article.create
          (Some item.Title)
          item.Link
          (item.Encoded |> Option.orElse item.Description)
          (Some item.PubDate)

    let private toArticles (rss: RssProvider.Rss) =
        unsafeOperation "Rss to articles" { return fun _ ->
            rss.Channel.Items
            |> Seq.map itemToArticle
            |> Seq.toList
        }

    let private parse xml: Result<RssProvider.Rss, string> =
        unsafeOperation "Rss parse" { return fun _ -> RssProvider.Parse xml }

    let processRss (DownloadedFeed downloaded): ProcessingResult =
        parse downloaded |> Result.bind toArticles


module private Atom =

    type private AtomProvider = XmlProvider<"../feeds-processing/Resources/samples/github.atom.sample">

    let private entryToArticle (entry: AtomProvider.Entry) =
        Article.create
            (Some entry.Title.Value)
            entry.Link.Href
            (Some entry.Content.Value)
            (Some entry.Published)

    let private toArticles (atom: AtomProvider.Feed) =
        unsafeOperation "Atom to articles" { return! fun _ ->
            match atom.Entries with
            | [||] ->
                Error "Expected at least one atom entry"
            | entries ->
                entries
                |> Seq.map entryToArticle
                |> Seq.toList
                |> Ok
        }

    let private parse xml: Result<AtomProvider.Feed, string> =
        unsafeOperation "Atom parse" { return fun _ -> AtomProvider.Parse xml }

    let processAtom (DownloadedFeed downloaded): ProcessingResult =
        parse downloaded |> Result.bind toArticles


module private Rdf =

    type private RdfProvider = XmlProvider<"../feeds-processing/Resources/samples/slashdot.rdf.sample">

    let private itemToArticle (item: RdfProvider.Item) =
        Article.create
            (Some item.Title)
            item.Link
            (Some item.Description)
            (Some item.Date)

    let private toArticles (rdf: RdfProvider.Rdf): Result<Article list, string> =
        unsafeOperation "Rdf to articles" { return! fun _ ->
            match rdf.Items with
            | [||] ->
                Error "Expected at least one rdf item"
            | items ->
                items
                |> Seq.map itemToArticle
                |> Seq.toList
                |> Ok
        }

    let private parse (xml: string): Result<RdfProvider.Rdf, string> =
        unsafeOperation "Rdf parse" { return fun _ -> RdfProvider.Parse xml }

    let processRdf (DownloadedFeed downloaded): ProcessingResult =
        parse downloaded |> Result.bind toArticles


type private Processor =
    DownloadedFeed -> ProcessingResult


let private tryProcessor downloaded (previousState: ProcessingResult) (processor: Processor): ProcessingResult =
    match previousState with
    | Ok articles -> Ok articles
    | Error msg ->
        match processor downloaded with
        | Ok articles -> Ok articles
        | Error nextMsg -> Error(sprintf "%s, %s" msg nextMsg)


let private processors: Processor list =
    [ Rss.processRss
      Atom.processAtom
      Rdf.processRdf
    ]

let processXmlFeed (downloaded: DownloadedFeed): ProcessingResult =
    (Error "", processors)
    ||> List.fold (tryProcessor downloaded)
    |> Result.mapError (sprintf "Failed all the parsers: %s")
