module FeedsProcessing.Atom

open System
open FSharp.Data
open FeedsProcessing
open FeedsProcessing.Feeds
open FeedsProcessing.ProcessingResult
open FeedsProcessing.Download
open FeedsProcessing.Article


type private AtomProvider = XmlProvider<"../feeds-processing/Resources/samples/github.atom.sample">


let private parse (xml : string) : Result<AtomProvider.Feed, string> =
    try
        Ok <| AtomProvider.Parse xml
    with
    | ex ->
        printfn "Could not parse Atom\n\n%s\n\nGot exception %s" xml (ex.ToString ())
        Error "Could not parse Atom"


let private entryToArticle (entry : AtomProvider.Entry) : Article =
    { Title = Xml.stringToOption entry.Title.Value
      Link = Xml.stringToOption entry.Link.Href
      Content = Xml.stringToOption entry.Content.Value |> Option.defaultValue ""
      Date = Some entry.Published
    }


let private atomToArticles (atom : AtomProvider.Feed) : Article list =
    atom.Entries
        |> Array.toList
        |> List.map entryToArticle


let processAtomFeed (DownloadedFeed downloaded) : ProcessingResult =
    parse downloaded |> Result.map atomToArticles
