module FeedsProcessing.Article

open System


let private stringToOption text =
    if String.IsNullOrWhiteSpace text
    then None
    else Some text


module private Html =
    open Ganss.XSS

    let private sanitizer =
        let s = new HtmlSanitizer()
        s.AllowedAttributes.Add("class") |> ignore
        s

    let sanitize (html : string) : string =
        sanitizer.Sanitize html


type private Fields =
    { Title : string option
      Link : string option
      Content : string
      Date : DateTimeOffset option
    }


type Article =
    private Article of Fields


let title (Article fields) = fields.Title
let link (Article fields) = fields.Link
let content (Article fields) = fields.Content
let date (Article fields) = fields.Date


let create (title : string option) (link : string) (content : string option) (date : DateTimeOffset option) : Article =
    Article
        { Title =
              title
              |> Option.bind stringToOption
          Link = stringToOption link
          Content =
              content
              |> Option.bind stringToOption
              |> Option.defaultValue ""
              |> Html.sanitize
          Date = date
        }
