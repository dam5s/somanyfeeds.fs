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
        | "About" -> About
        | "Social" -> Social
        | "Code" -> Code
        | _ -> Blog

    let toString (source: Source) =
        sprintf "%A" source
