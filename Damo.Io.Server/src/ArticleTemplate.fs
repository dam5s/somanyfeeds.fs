[<RequireQualifiedAccess>]
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
    | 10  -> "October"
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

open Giraffe.GiraffeViewEngine

let private articleTitle (article: Article) title =
    match article.Link with
    | Some link -> h1 [] [ a [ _href link ] [ str title ] ]
    | None -> h1 [] [ str title ]

let private articleDate (date: Posix) =
    h2 [ _class "date" ] [ str (dateToString date) ]

let private articleHeader (article: Article) =
    header []
        (List.choose id
            [ Option.map (articleTitle article) article.Title
              Option.map articleDate article.Date ]
        )

let render (a: Article): XmlNode =
    let className =
        a.Title
        |> Option.map (always "with-title")
        |> Option.defaultValue "no-title"

    article [ _class className ]
        [ articleHeader a
          section [] [ rawText a.Content ]
        ]
