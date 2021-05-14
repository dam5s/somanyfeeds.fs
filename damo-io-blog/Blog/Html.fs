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
                    title [] [ str pageTitle ]
                  ]
              body [] [
                  header [] [
                      h1 [] [ str pageTitle ]
                      nav [] [ a [ Href "/" ] [ str "Home" ] ]
                  ]
                  content
                  footer [] [ str $"Copyright © %s{name} %d{year}" ]
              ]
            ]
        |> Fable.ReactServer.renderToString

    let private contentsItem post =
        article [ Class "contentsItem" ]
            [ a [ Href (Post.path post) ]
                  [ h3 [] [ str post.Title  ]
                  ]
            ]

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

        main [ Class "tableOfContents" ]
            [ nav [] (tags |> List.map tagLink)
              section [] (contentsList posts)
            ]
        |> withLayout

    let tagPage (tag: string) (posts: Post list) =
        let postList = contentsList posts

        [ h2 [] [ str $"Tag: %s{tag}" ]  ] @ postList
        |> main [ Class "tagPage" ]
        |> withLayout

    let postPage (post: Post) =
        main [ Class "postPage" ] [
            article [ DangerouslySetInnerHTML { __html = post.HtmlContent } ] []
        ]
        |> withLayout
