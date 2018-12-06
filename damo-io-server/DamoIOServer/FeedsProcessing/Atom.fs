module DamoIOServer.FeedsProcessing.Atom

open System
open FSharp.Data
open DamoIOServer.FeedsProcessing.ProcessingResult
open DamoIOServer.FeedsProcessing.Download
open DamoIOServer.Articles.Data
open DamoIOServer.SourceType


type private AtomProvider = XmlProvider<"../damo-io-server/Resources/samples/github.atom.sample">


let private parse (xml : string) : Result<AtomProvider.Feed, string> =
    try
        Ok <| AtomProvider.Parse xml
    with
    | ex ->
        printfn "Could not parse Atom\n\n%s\n\nGot exception %s" xml (ex.ToString ())
        Error "Could not parse Atom"


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
