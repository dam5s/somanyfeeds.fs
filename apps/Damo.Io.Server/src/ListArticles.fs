module Damo.Io.Server.ListArticles

open FeedsPersistence.ArticleRecord

[<RequireQualifiedAccess>]
module ListArticles =
    let private merge now first second =
        { first with
            Date = Some(max (Option.defaultValue now first.Date) (Option.defaultValue now second.Date))
            Content = second.Content + first.Content }

    let rec mergeConsecutiveArticles now =
        function
        | [] -> []
        | [ x ] -> [ x ]
        | first :: second :: rest ->
            let sameSource = first.FeedName = second.FeedName
            let sameTitle = first.Title.IsSome && first.Title = second.Title

            if sameSource && sameTitle then
                let merged = merge now first second
                mergeConsecutiveArticles now (merged :: rest)
            else
                first :: mergeConsecutiveArticles now (second :: rest)
