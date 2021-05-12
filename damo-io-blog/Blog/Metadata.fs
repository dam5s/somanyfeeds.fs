module Blog.Metadata

open System

type Metadata =
    { Posted: DateTimeOffset
      Tags: string list }

[<RequireQualifiedAccess>]
module Metadata =
    open System.IO
    open FSharp.Data
    open OptionBuilder

    [<Literal>]
    let private example = """
        { "posted": "2021-05-04T23:59:59Z", "tags": [ "fsharp", "elm" ] }
    """

    type private MetadataProvider = JsonProvider<example>

    let private load (file: FileInfo) =
        MetadataProvider.Load (file.OpenText ())

    let tryGet (dir: DirectoryInfo): Metadata option =
        option {
            let path = $"%s{dir.FullName}/metadata.json"
            let! file = FileInfo.tryGet path
            let! json = Try.toOption load file

            let data =
                { Posted = json.Posted
                  Tags = json.Tags |> Array.toList }

            return data
        }
