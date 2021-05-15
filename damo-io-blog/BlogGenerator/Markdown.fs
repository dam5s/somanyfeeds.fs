module BlogGenerator.Markdown

type Markdown =
    { Title: string
      HtmlContent: string }

[<RequireQualifiedAccess>]
module Markdown =
    open System.IO
    open OptionBuilder
    open FSharp.Formatting.Markdown

    let private load (file: FileInfo) =
        file
        |> FileInfo.readAll
        |> Markdown.Parse

    let private tryGetTitle (paragraph: MarkdownParagraph) =
        match paragraph with
        | Heading(size=1; body=[Literal(text=text)]) -> Some text
        | _ -> None

    let private tryFindTitle (doc: MarkdownDocument) =
        doc.Paragraphs
        |> Seq.choose tryGetTitle
        |> Seq.tryHead

    let tryGet (dir: DirectoryInfo) =
        option {
            let path = $"{dir.FullName}/index.md"
            let! file = FileInfo.tryGet path
            let! doc = Try.toOption load file
            let! title = tryFindTitle doc

            let markdown =
                { Title = title
                  HtmlContent = Markdown.ToHtml(doc) }

            return! Some markdown
        }
