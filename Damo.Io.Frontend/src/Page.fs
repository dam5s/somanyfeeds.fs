module DamoIOFrontend.Page

open Source

type Page =
    | NoSourcesSelected
    | SelectedSources of Source list
    | NotFound

[<RequireQualifiedAccess>]
module Page =
    let private sourcesFromPath (path: string) =
        path.Replace("/", "").Split(",")
        |> Array.toList
        |> List.choose Source.fromString

    let sources page =
        match page with
        | NoSourcesSelected -> []
        | SelectedSources s -> s
        | NotFound -> []

    let fromPath (path: string) =
        match sourcesFromPath path with
        | [] -> NoSourcesSelected
        | s -> SelectedSources s
