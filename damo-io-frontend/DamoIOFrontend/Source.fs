module DamoIOFrontend.Source

type Source =
    | About
    | Social
    | Code
    | Blog

[<RequireQualifiedAccess>]
module Source =
    let all = [ About; Social; Code; Blog ]

    let fromString value =
        match value with
        | "About" -> Some About
        | "Social" -> Some Social
        | "Code" -> Some Code
        | "Blog" -> Some Blog
        | _ -> None

    let toString (source: Source) =
        $"%A{source}"
