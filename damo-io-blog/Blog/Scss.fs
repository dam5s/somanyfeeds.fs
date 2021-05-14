module Blog.Scss

open System.IO

[<RequireQualifiedAccess>]
module Scss =
    open SharpScss

    let private convertFile (file: FileInfo) =
        let options = ScssOptions(OutputStyle = ScssOutputStyle.Compressed)
        Scss.ConvertFileToCss(file.FullName, options).Css

    let convert path =
        path
        |> FileInfo.tryGet
        |> Option.map convertFile
        |> Option.defaultValue ""
