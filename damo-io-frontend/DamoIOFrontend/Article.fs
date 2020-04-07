module DamoIOFrontend.Article

open SoManyFeedsFrontend.Support
open Source

type Article =
    { Title: string option
      Link: string option
      Content: string
      Date: Time.Posix option
      Source: Source }

[<RequireQualifiedAccess>]
module Article =
    type Json =
        { Title: string option
          Link: string option
          Content: string
          Date: int64 option
          Source: string }

    let fromJson json: Article =
        { Title = json.Title
          Link = json.Link
          Content = json.Content
          Date = json.Date |> Option.map (int64 >> Time.Posix)
          Source = json.Source |> Source.fromString }

    open Fable.React
    open Fable.React.Props

    let private articleTitle (article: Article) title =
        match article.Link with
        | Some link -> h1 [] [ a [ Href link ] [ str title ] ]
        | None -> h1 [] [ str title ]

    let private articleDate (date: Time.Posix) =
        h2 [ Class "date" ] [ str (Posix.toString date) ]

    let private articleHeader (article: Article) =
        header []
            (List.choose id
                [ Option.map (articleTitle article) article.Title
                  Option.map articleDate article.Date ]
            )

    let view (article: Article): ReactElement =
        match article.Title with
        | Some t ->
            Standard.article []
                [ articleHeader article
                  section [ Html.rawInnerHtml article.Content ] []
                ]
        | None ->
            Standard.article [ Class "tweet" ]
                [ articleHeader article
                  Tweet.display article.Content
                ]

    let private defaultContent =
        """
        <p>
        You have deselected all the feeds in the menu. There is nothing to show.
        Feel free to select one or more feeds to display more entries.
        </p>
        """

    let defaultArticle: Article =
        { Title = Some "Nothing to see here."
          Link = Some "https://damo.io"
          Content = defaultContent
          Date = None
          Source = Source.About }

    let private aboutContent =
        """
        <p>
          I'm <strong>Damien Le Berrigaud</strong>, Software Engineer for
          <a href="https://pivotal.io/">Pivotal Software</a>.

          I like working with
          <a href="https://kotlinlang.org">Kotlin</a>,
          <a href="https://fsharp.org">F#</a>,
          <a href="http://elm-lang.org">Elm</a>,
          <a href="https://developer.android.com">Android</a>,
          <a href="https://developer.apple.com/ios">iOS (Swift or Objective-C)</a>,
          <a href="https://golang.org">Go</a>...

          I've had previous experiences in
          <a href="https://www.ruby-lang.org">Ruby</a> and
          <a href="https://www.microsoft.com/net">.NET</a>.
        </p>
        <p>
          That's a lot of technologies that I got to work with while working at <em>Pivotal Labs</em>
          and in my previous jobs.
          In order to keep up-to-date, I keep rebuilding this website.
        </p>
        <p>
          This version is written in <em>F#</em> for the frontend and for the backend running on the <em>.NET Core</em> platform.
          The source code is entirely available <a href="https://github.com/dam5s/somanyfeeds.fs">on my github</a>.
        </p>
        <p>
          You can choose to follow multiple of my feeds, on a single page, by using the menu at the top of the page.
        </p>
        """

    let about: Article =
        { Title = Some "About"
          Link = None
          Content = aboutContent
          Date = None
          Source = Source.About }
