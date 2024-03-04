module FeedsProcessing.Xml

open FSharp.Data
open System

open FeedsProcessing.Article
open FeedsProcessing.Download
open FeedsProcessing.ProcessingResult

let private stringToOption text =
    if String.IsNullOrWhiteSpace text then None else Some text

type FeedMetadata =
    { Url: Url
      Title: string
      Description: string }

type Processor =
    { Process: Download -> ProcessingResult
      TryGetMetadata: Download -> FeedMetadata option }

[<RequireQualifiedAccess>]
module private Processor =
    let create
        name
        (unsafeParse: string -> 'a)
        (toArticles: 'a -> ProcessingResult)
        (toMetadata: Url -> 'a -> FeedMetadata option)
        =

        let safeParse (download: Download) =
            Try.value $"%s{name} parse" (fun _ -> unsafeParse download.Content)

        { Process = fun download -> safeParse download |> Result.bind toArticles
          TryGetMetadata =
            fun download -> safeParse download |> Result.toOption |> Option.bind (toMetadata download.Url) }


module private Rss =

    type private RssProvider = XmlProvider<"../FeedsProcessing/resources/samples/rss.sample.xml">

    let private descriptionToString (description: RssProvider.Description) = description.Value

    let private contentToMedia (content: RssProvider.Content) =
        { Url = content.Url
          Description = content.Description |> Option.map descriptionToString |> Option.defaultValue "" }

    let private itemToArticle (item: RssProvider.Item) =
        Article.create
            item.Title
            item.Link
            (item.Encoded |> Option.orElse item.Description)
            (item.Content |> Option.map contentToMedia)
            (Some item.PubDate)

    let private toArticles (rss: RssProvider.Rss) =
        Try.value "Rss to articles" (fun _ -> rss.Channel.Items |> Seq.map itemToArticle |> Seq.toList)

    let private toMetadata (url: Url) (rss: RssProvider.Rss) =
        try
            Some
                { Title = rss.Channel.Title
                  Description = rss.Channel.Description
                  Url = url }
        with _ ->
            None

    let processor = Processor.create "Rss" RssProvider.Parse toArticles toMetadata


module private Atom =

    type private AtomProvider = XmlProvider<"../FeedsProcessing/resources/samples/atom.sample.xml">

    let private entryToArticle (entry: AtomProvider.Entry) =
        Article.create
            (Some entry.Title.Value)
            (entry.Links |> Array.head |> (fun l -> l.Href))
            (Some entry.Content.Value)
            None
            (Some entry.Published)

    let private toArticles (atom: AtomProvider.Feed) =
        Try.result
            "Atom to articles"
            (fun _ ->
                match atom.Entries with
                | [||] -> Error.ofMessage "Expected at least one atom entry"
                | entries -> entries |> Seq.map entryToArticle |> Seq.toList |> Ok
            )

    let private toMetadata (url: Url) (atom: AtomProvider.Feed) =
        try
            Some
                { Title = atom.Title
                  Description = ""
                  Url = url }
        with _ ->
            None

    let processor = Processor.create "Atom" AtomProvider.Parse toArticles toMetadata


module private Rdf =

    type private RdfProvider = XmlProvider<"../FeedsProcessing/resources/samples/rdf.sample.xml">

    let private itemToArticle (item: RdfProvider.Item) =
        Article.create (Some item.Title) item.Link (Some item.Description) None (Some item.Date)

    let private toArticles (rdf: RdfProvider.Rdf) : Result<Article list, Explanation> =
        Try.result
            "Rdf to articles"
            (fun _ ->
                match rdf.Items with
                | [||] -> Error.ofMessage "Expected at least one rdf item"
                | items -> items |> Seq.map itemToArticle |> Seq.toList |> Ok
            )

    let private toMetadata (url: Url) (rdf: RdfProvider.Rdf) =
        try
            Some
                { Title = rdf.Channel.Title
                  Description = rdf.Channel.Description
                  Url = url }
        with _ ->
            None

    let processor = Processor.create "Rdf" RdfProvider.Parse toArticles toMetadata



let private tryProcessor downloaded (previousState: ProcessingResult) (processor: Processor) : ProcessingResult =
    match previousState with
    | Ok articles -> Ok articles
    | Error err ->
        match processor.Process downloaded with
        | Ok articles -> Ok articles
        | Error nextErr -> Error(Explanation.append err nextErr)

let private processors: Processor list =
    [ Rss.processor; Atom.processor; Rdf.processor ]

[<RequireQualifiedAccess>]
module Xml =
    let processFeed (download: Download) : ProcessingResult =
        (Error.ofMessage "", processors)
        ||> List.fold (tryProcessor download)
        |> Result.mapError (Explanation.wrapMessage (sprintf "Failed all the parsers: %s"))

    let tryGetMetadata (download: Download) : FeedMetadata option =
        processors |> List.choose (fun p -> p.TryGetMetadata download) |> List.tryHead
