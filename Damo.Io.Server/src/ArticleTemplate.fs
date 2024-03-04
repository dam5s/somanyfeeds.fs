module DamoIoServer.ArticleTemplate

open Time

open DamoIoServer.Article

let private monthDisplayName month =
    match month with
    | 1 -> "January"
    | 2 -> "February"
    | 3 -> "March"
    | 4 -> "April"
    | 5 -> "May"
    | 6 -> "June"
    | 7 -> "July"
    | 8 -> "August"
    | 9 -> "September"
    | 10 -> "October"
    | 11 -> "November"
    | 12 -> "December"
    | x -> failwithf $"Not a valid month rank: %i{x}"

let private dateToString posix =
    let date = Posix.toDateTime posix
    let month = monthDisplayName date.Month
    let day = date.ToString "dd"
    let year = date.ToString "yy"
    let time = date.ToString "HH:mm"

    $"%s{month} %s{day} '%s{year} @ %s{time}"

open Giraffe.ViewEngine

let private articleTitle (article: ArticleRecord) title =
    match article.Link with
    | Some link -> h1 [] [ a [ _href link ] [ str title ] ]
    | None -> h1 [] [ str title ]

let private articleDate (date: Posix) =
    h2 [ _class "date" ] [ str (dateToString date) ]

let private articleHeader (article: ArticleRecord) =
    header
        []
        (List.choose
            id
            [ Option.map (articleTitle article) article.Title
              Option.map articleDate article.Date ])

let private trySourceLink (article: ArticleRecord) : XmlNode option =
    match (article.Title, article.Link) with
    | None, Some url -> Some(nav [] [ a [ _href url; _target "_blank" ] [ str "Source" ] ])
    | _, _ -> None

let private renderMedia (media: MediaRecord) : XmlNode =
    figure
        []
        [ img [ _src media.Url; _alt media.Description ]
          figcaption [] [ str media.Description ] ]

[<RequireQualifiedAccess>]
module ArticleTemplate =
    let render (article: ArticleRecord) : XmlNode =
        let articleHeader = articleHeader article
        let articleContent = section [] [ rawText article.Content ]
        let maybeSourceLink = trySourceLink article

        let sourceNameClass = article.SourceName.Replace(" ", "")
        let sourceTypeClass = article.SourceType.ToString()
        let cssClasses = $"{sourceNameClass} {sourceTypeClass}"

        HtmlElements.article
            [ _class cssClasses ]
            [ yield articleHeader
              yield articleContent

              match article.Media with
              | Some media -> yield renderMedia media
              | None -> ()

              match maybeSourceLink with
              | Some sourceLink -> yield sourceLink
              | None -> () ]
