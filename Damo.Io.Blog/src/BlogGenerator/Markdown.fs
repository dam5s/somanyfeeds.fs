module BlogGenerator.Markdown

open FSharp.Formatting.Markdown

type Markdown =
    { Title: string
      HtmlContent: string
      RssContent: string }

module private MarkdownSpans =
    let map f (spans: MarkdownSpans) =
        spans |> List.map f

module private rec MarkdownParagraphs =
    let private mapParagraphSpans f (paragraph: MarkdownParagraph) =
        let map = MarkdownSpans.map f

        match paragraph with
        | Heading (size, body, range) -> Heading (size, map body, range)
        | Paragraph (body, range) -> Paragraph (map body, range)
        | ListBlock (kind, items, range) -> ListBlock (kind, items |> List.map (mapSpans f), range)
        | QuotedBlock (paragraphs, range) -> QuotedBlock (mapSpans f paragraphs, range)
        | Span (body, range) -> Span (map body, range)
        | unsupported -> unsupported

    let mapSpans f (paragraphs: MarkdownParagraphs) =
        paragraphs |> List.map (mapParagraphSpans f)

[<RequireQualifiedAccess>]
module Markdown =
    open System.IO
    open OptionBuilder

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

    let private removeTitle title paragraphs =
        paragraphs
        |> List.filter (fun p ->
            match tryGetTitle p with
            | Some t -> t <> title
            | _ -> true
        )

    let prefixImageUrl prefix (span: MarkdownSpan) =
        match span with
        | DirectImage (body, link, title, range) -> DirectImage (body, prefix + link, title, range)
        | IndirectImage (body, link, key, range) -> IndirectImage (body, prefix + link, key, range)
        | notImage -> notImage

    let rssParagraphs urlPrefix title (paragraphs: MarkdownParagraphs): MarkdownParagraphs =
        let spanMapping = prefixImageUrl urlPrefix

        paragraphs
        |> removeTitle title
        |> MarkdownParagraphs.mapSpans spanMapping

    let tryGet urlPrefix (dir: DirectoryInfo) =
        option {
            let path = $"{dir.FullName}/index.md"
            let! file = FileInfo.tryGet path
            let! doc = Try.toOption load file
            let! title = tryFindTitle doc
            let rss =
                MarkdownDocument(
                    rssParagraphs urlPrefix title doc.Paragraphs,
                    doc.DefinedLinks
                )

            let markdown =
                { Title = title
                  HtmlContent = Markdown.ToHtml(doc)
                  RssContent = Markdown.ToHtml(rss) }

            return! Some markdown
        }
