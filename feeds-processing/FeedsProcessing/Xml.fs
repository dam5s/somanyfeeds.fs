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
    try
        Ok <| operation ()
    with
    | ex ->
        Error ex.Message


module private Rss =

    type private RssProvider = XmlProvider<"../feeds-processing/Resources/samples/medium.rss.sample">


    let private itemToArticle (item : RssProvider.Item) : Article =
        { Title = stringToOption item.Title
          Link = stringToOption item.Link
          Content = stringToOption item.Encoded |> Option.defaultValue ""
          Date = Some item.PubDate
        }


    let private toArticles (rss : RssProvider.Rss) : Result<Article list, string> =
        tryOperation (fun _ ->
            rss.Channel.Items
                |> Array.toList
                |> List.map itemToArticle
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
        tryOperation (fun _ ->
            atom.Entries
                |> Array.toList
                |> List.map entryToArticle
        )


    let private parse (xml : string) : Result<AtomProvider.Feed, string> =
        tryOperation (fun _ -> AtomProvider.Parse xml)


    let processAtom (DownloadedFeed downloaded) : ProcessingResult =
        parse downloaded |> Result.bind toArticles


let processXmlFeed (downloaded : DownloadedFeed) : ProcessingResult =
    match Rss.processRss downloaded with
    | Ok articles -> Ok articles
    | Error rssErr ->
        match Atom.processAtom downloaded with
        | Ok articles -> Ok articles
        | Error atomErr ->
            printfn "Feed failed both parsers. Rss: %s, Atom: %s" rssErr atomErr
            Error (sprintf "RSS error: %s, Atom error: %s" rssErr atomErr)
