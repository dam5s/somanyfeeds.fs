module Server.FeedsProcessing.Atom

open System
open FSharp.Data
open Server.FeedsProcessing.ProcessingResult
open Server.FeedsProcessing.Download
open Server.Articles.Data
open Server.SourceType


type private AtomProvider = XmlProvider<"../server/resources/github.atom.xml">


let private parse (xml : string) : Result<AtomProvider.Feed, string> =
    try
        Ok <| AtomProvider.Parse(xml)
    with
    | ex ->
        printfn "Could not parse Atom\n\n%s\n\nGot exception %s" xml (ex.ToString())
        Error <| String.Format("Could not parse Atom")


let private entryToRecord (source : SourceType) (entry : AtomProvider.Entry) : Record =
    { Title = Xml.stringToOption entry.Title.Value
      Link = Xml.stringToOption entry.Link.Href
      Content = Xml.stringToOption entry.Content.Value |> Option.defaultValue ""
      Date = Some entry.Published
      Source = source
    }


let private atomToRecords (source : SourceType) (atom : AtomProvider.Feed) : Record list =
    atom.Entries
        |> Array.toList
        |> List.map (entryToRecord source)


let processAtomFeed (source : SourceType) (downloaded : DownloadedFeed) : ProcessingResult =
    parse (downloadedString downloaded) |> Result.map (atomToRecords source)
