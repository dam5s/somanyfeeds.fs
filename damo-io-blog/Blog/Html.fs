module Blog.Html

open System
open Blog.Posts

[<RequireQualifiedAccess>]
module Html =
    open Fable.React
    open Fable.React.Props

    let private withLayout content =
        let name = "Damien Le Berrigaud"
        let year = DateTimeOffset.Now.Year
        let pageTitle = $"%s{name}'s Blog"

        html [ Lang "en" ]
            [ head []
                  [ title [] [ str pageTitle ] ]
              body [] [
                  header [ Id "main" ] [
                      h1 [] [ str pageTitle ]
                  ]
                  content
                  footer [ Id "main" ] [ str $"Copyright © %s{name} %d{year}" ]
              ]
            ]
        |> Fable.ReactServer.renderToString

    let private contentsItem post =
        article [ Class "contentsItem" ]
            [ a [ Href $"%s{post.Slug}/" ]
                  [ h3 [] [ str post.Title  ]
                  ]
            ]

    let tableOfContents (posts: Post list) =
        let postList =
            posts
            |> List.sortByDescending (fun p -> p.Posted)
            |> List.map contentsItem

        postList
        |> section [ Id "main"; Class "tableOfContents" ]
        |> withLayout

    let private postHtml post =
        article [ DangerouslySetInnerHTML { __html = post.HtmlContent } ] []

    let postPage (post: Post) =
        [ postHtml post ]
        |> section [ Id "main"; Class "postPage" ]
        |> withLayout

    let tagPage (tag: string) (posts: Post list) =
        let postsHtml =
            posts
            |> List.map postHtml

        [ h2 [] [ str $"Tag: %s{tag}" ]  ] @ postsHtml
        |> section [ Id "main"; Class "tagPage" ]
        |> withLayout
