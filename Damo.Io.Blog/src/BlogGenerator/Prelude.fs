[<AutoOpen>]
module BlogGenerator.Prelude

let always a _ = a

module OptionBuilder =
    type OptionBuilder() =
        member _.Bind(opt, f) = Option.bind f opt
        member _.Return(value) = Some value
        member _.ReturnFrom(option) = option

    let option = OptionBuilder()

[<RequireQualifiedAccess>]
module Try =
    let toOption f a =
        try Some (f a)
        with _ -> None

[<RequireQualifiedAccess>]
module FileInfo =
    open System.IO

    let tryGet path =
        if File.Exists(path)
        then Some (FileInfo path)
        else None

    let readAll (file: FileInfo) =
        file.FullName
        |> File.ReadLines
        |> String.concat "\n"
