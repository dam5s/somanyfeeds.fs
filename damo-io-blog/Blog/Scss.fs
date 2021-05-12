module Blog.Scss

[<RequireQualifiedAccess>]
module Scss =
    open SharpScss

    let private options =
        ScssOptions(OutputStyle = ScssOutputStyle.Compressed)

    let private convertCss css =
        Scss.ConvertToCss(css, options).Css

    let private convertFile file =
        file
        |> FileInfo.readAll
        |> convertCss

    let convert path =
        path
        |> FileInfo.tryGet
        |> Option.map convertFile
        |> Option.defaultValue ""
