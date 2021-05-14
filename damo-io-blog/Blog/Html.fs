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
                  [ meta [ HTMLAttr.Content "text/html;charset=utf-8"; HttpEquiv "Content-Type" ]
                    meta [ HTMLAttr.Content "utf-8"; HttpEquiv "encoding" ]
                    meta [ Name "viewport";  HTMLAttr.Content "width=device-width" ]
                    link [ Rel "stylesheet"; Type "text/css"; Href "/app.css" ]
                    title [] [ str pageTitle ]
                  ]
              body [] [
                  header [] [
                      h1 [] [ str pageTitle ]
                      nav [] [ a [ Href "/" ] [ str "Home" ] ]
                  ]
                  main [] content
                  footer [] [ str $"© %d{year} — %s{name}" ]
              ]
            ]
        |> Fable.ReactServer.renderToString

    let private contentsItem post =
        h3 [] [ a [ Href (Post.path post) ] [ str post.Title  ] ]

    let private contentsList posts =
        posts
        |> List.sortByDescending (fun p -> p.Posted)
        |> List.map contentsItem

    let private tagLink tag =
        a [ Href $"tags/%s{tag}" ] [ str tag  ]

    let tableOfContents (posts: Post list) =
        let tags =
            posts
            |> Posts.tags
            |> Set.toList
            |> List.sort

        withLayout [
          nav [] (tags |> List.map tagLink)
          section [] (contentsList posts)
        ]


    let tagPage (tag: string) (posts: Post list) =
        let postList = contentsList posts

        withLayout (
            [ h1 [] [ str $"Posts tagged with “%s{tag}”" ]  ] @ postList
        )

    let postPage (post: Post) =
        withLayout [
            article [ DangerouslySetInnerHTML { __html = post.HtmlContent } ] []
        ]
