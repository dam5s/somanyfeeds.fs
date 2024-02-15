module FeedsProcessing.Article

open FeedsProcessing
open System
open Time

let private stringToOption text =
    if String.IsNullOrWhiteSpace text then None else Some text

type Article = private Article of Fields

and private Fields =
    { Title: string option
      Link: string option
      Content: string
      Media: Media option
      Date: Posix option }

and Media = { Url: string; Description: string }

[<RequireQualifiedAccess>]
module Article =
    let title (Article fields) = fields.Title
    let link (Article fields) = fields.Link
    let content (Article fields) = fields.Content
    let media (Article fields) = fields.Media
    let date (Article fields) = fields.Date

    let create title link content media date =
        Article
            { Title = title |> Option.bind stringToOption
              Link = stringToOption link
              Content = content |> Option.bind stringToOption |> Option.defaultValue "" |> Html.sanitize
              Media = media
              Date = Option.map Posix.fromDateTimeOffset date }
