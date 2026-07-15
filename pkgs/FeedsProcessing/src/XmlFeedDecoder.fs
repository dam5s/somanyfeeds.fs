module FeedsProcessing.XmlFeedDecoder

open System.Threading.Tasks
open FSharp.Data
open System

open FeedsProcessing.Article
open FeedsProcessing.Download
open Microsoft.Extensions.Logging

let private stringToOption text =
    if String.IsNullOrWhiteSpace text then None else Some text

type IXmlFeedDecoder =
    abstract member Decode: Download -> Article list

type private RssProvider = XmlProvider<"../FeedsProcessing/resources/samples/rss.sample.xml">

type private RssDecoder() =

    let descriptionToString (description: RssProvider.Description) = description.Value

    let contentToMedia (content: RssProvider.Content) =
        { Url = content.Url
          Description = content.Description |> Option.map descriptionToString |> Option.defaultValue "" }

    let itemToArticle (item: RssProvider.Item) =
        Article.create
            item.Title
            item.Link
            (item.Encoded |> Option.orElse item.Description)
            (item.Content |> Option.map contentToMedia)
            (Some item.PubDate)

    interface IXmlFeedDecoder with
        member _.Decode download =
            let rss = RssProvider.Parse download.Content
            rss.Channel.Items |> Seq.map itemToArticle |> Seq.toList

type private AtomProvider = XmlProvider<"../FeedsProcessing/resources/samples/atom.sample.xml">

type private AtomDecoder() =
    let tryParseDateTimeOffset (text: String) : DateTimeOffset option =
        let normalized = text.Replace(" UTC", "+00:00")

        match DateTimeOffset.TryParse(normalized) with
        | true, dateTimeOffset -> Some dateTimeOffset
        | _ -> None

    let publishedDateTimeOffset (published: AtomProvider.Published) : DateTimeOffset option =
        match (published.DateTime, published.String) with
        | Some dateTimeOffset, _ -> Some dateTimeOffset
        | _, Some string -> tryParseDateTimeOffset string
        | None, None -> None

    let entryToArticle (entry: AtomProvider.Entry) =
        Article.create
            (Some entry.Title.Value)
            (entry.Links |> Array.head |> (fun l -> l.Href))
            (Some entry.Content.Value)
            None
            (publishedDateTimeOffset entry.Published)

    interface IXmlFeedDecoder with
        member _.Decode download =
            let atom = AtomProvider.Parse download.Content

            match atom.Entries with
            | [||] -> failwith "Expected at least one atom entry"
            | entries -> entries |> Seq.map entryToArticle |> Seq.toList



type XmlFeedDecoder(logger: ILogger<XmlFeedDecoder>) =
    let rss = RssDecoder()
    let atom = AtomDecoder()

    let tryWithDecoder download (decoder: IXmlFeedDecoder) : Result<Article list, exn list> =
        try
            Ok(decoder.Decode download)
        with ex ->
            Error [ ex ]

    let tryWithDecoderAsync download (decoder: IXmlFeedDecoder) : TaskResult<Article list> =
        Task.Run(fun () -> tryWithDecoder download decoder)

    let logErrors msg (errors: exn list) =
        for ex in errors do
            logger.LogError(ex, msg)

    member _.TryDecodeAsync(download: Download) : TaskResult<Article list> =
        task {
            let rssTask = tryWithDecoderAsync download rss
            let atomTask = tryWithDecoderAsync download atom

            let! _ = Task.WhenAll(rssTask, atomTask)

            match rssTask.Result, atomTask.Result with
            | Ok x, _ -> return Ok x
            | _, Ok x -> return Ok x
            | Error rssEx, Error atomEx ->
                let errors = (rssEx @ atomEx)
                logger.LogErrors(errors, $"Failed to parse XML feed: {download.Url}")
                return Error errors
        }
