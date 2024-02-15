module BlogGenerator.Html

open System
open BlogGenerator.Posts
open BlogGenerator.Config

[<RequireQualifiedAccess>]
module Html =
    open Fable.React
    open Fable.React.Props

    let private withLayout (config: Config) subtitle content =
        let year = DateTimeOffset.Now.Year

        html
            [ Lang "en" ]
            [ head
                  []
                  [ meta [ HTMLAttr.Content "text/html;charset=utf-8"; HttpEquiv "Content-Type" ]
                    meta [ HTMLAttr.Content "utf-8"; HttpEquiv "encoding" ]
                    meta [ Name "viewport"; HTMLAttr.Content "width=device-width" ]
                    link [ Rel "stylesheet"; Type "text/css"; Href "/app.css" ]
                    link [ Rel "alternate"; Type "application/rss+xml"; Href "/rss.xml" ]
                    link [ Rel "icon"; Type "image/svg+xml"; Sizes "any"; Href "/favicon.svg" ]
                    title [] [ str $"{config.Title} - %s{subtitle}" ] ]
              body
                  []
                  [ header [] [ h1 [] [ str config.Title ]; nav [] [ a [ Href "/" ] [ str "Home" ] ] ]
                    main [] content
                    footer [] [ str $"© %d{year} — %s{config.Author}" ] ] ]
        |> Fable.ReactServer.renderToString

    let private contentsItem post =
        p
            []
            [ a [ Href(Post.path post) ] [ str post.Title ]
              str $", %s{Post.displayDate post}" ]

    let private contentsList posts =
        posts |> List.sortByDescending (fun p -> p.Posted) |> List.map contentsItem

    let private tagLink tag = a [ Href $"tags/%s{tag}" ] [ str tag ]

    let tableOfContents config (posts: Post list) =
        let tags = posts |> Posts.tags |> Set.toList |> List.sort

        withLayout config "Table of Contents" [ nav [] (tags |> List.map tagLink); section [] (contentsList posts) ]

    let tagPage config (tag: string) (posts: Post list) =
        let postList = contentsList posts
        let title = $"Posts tagged with “%s{tag}”"

        withLayout config title ([ h1 [] [ str title ] ] @ postList)

    let postPage config (post: Post) =
        withLayout
            config
            post.Title
            [ article
                  []
                  [ aside [] [ str $"posted on %s{Post.displayDate post}" ]
                    section [ DangerouslySetInnerHTML { __html = post.HtmlContent } ] [] ] ]
