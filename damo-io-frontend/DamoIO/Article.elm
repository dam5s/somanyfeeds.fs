module DamoIO.Article exposing (Article, Json, about, default, fromJson, view)

import DamoIO.Source as Source exposing (Source(..))
import DamoIO.Tweet as Tweet
import Html exposing (Attribute, Html, a, h1, h2, header, section, text)
import Html.Attributes exposing (class, href)
import Regex
import Support.DateFormat as DateFormat
import Support.RawHtml as RawHtml
import Time


type alias Json =
    { title : Maybe String
    , link : Maybe String
    , content : String
    , date : Maybe Int
    , source : String
    }


type alias Article =
    { title : Maybe String
    , link : Maybe String
    , content : String
    , date : Maybe Time.Posix
    , source : Source
    }


fromJson : Json -> Article
fromJson json =
    { title = json.title
    , link = json.link
    , content = json.content
    , date = json.date |> Maybe.map Time.millisToPosix
    , source = Source.fromString json.source
    }


view : Maybe Time.Zone -> Article -> Html msg
view timeZone article =
    case article.title of
        Just t ->
            Html.article []
                [ articleHeader timeZone article
                , section [] <| RawHtml.fromString article.content
                ]

        Nothing ->
            Html.article [ class "tweet" ]
                [ articleHeader timeZone article
                , Tweet.display article.content
                ]


articleHeader : Maybe Time.Zone -> Article -> Html msg
articleHeader timeZone article =
    header []
        (compact
            [ Maybe.map (articleTitle article) article.title
            , Maybe.map (articleDate timeZone) article.date
            ]
        )


articleDate : Maybe Time.Zone -> Time.Posix -> Html msg
articleDate timeZone date =
    h2 [ class "date" ] [ text (DateFormat.tryFormat timeZone date) ]


articleTitle : Article -> String -> Html msg
articleTitle model title =
    case model.link of
        Just link ->
            h1 []
                [ a [ href link ] [ text <| titleText title ] ]

        Nothing ->
            h1 []
                [ text <| titleText title ]


titleText : String -> String
titleText title =
    let
        regex =
            Regex.fromString "\\shttp(s?)://[^\\s]*" |> Maybe.withDefault Regex.never
    in
    Regex.replace regex (\_ -> "") title


compact : List (Maybe a) -> List a
compact list =
    List.filterMap identity list


default : Article
default =
    { title = Just "Nothing to see here."
    , link = Just "http://damo.io"
    , content = defaultContent
    , date = Nothing
    , source = About
    }


defaultContent : String
defaultContent =
    """
  <p>
    You have deselected all the feeds in the menu. There is nothing to show.
    Feel free to select one or more feeds to display more entries.
  </p>
"""


about : Article
about =
    { title = Just "About"
    , link = Nothing
    , content = aboutContent
    , date = Nothing
    , source = About
    }


aboutContent : String
aboutContent =
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
    That's a lot of technologies that I got to work with while working at <em>Pivotal Labs</em> and in my previous
    jobs.
    In order to keep up-to-date, I keep rebuilding this website.
  </p>

  <p>
    This version is written in <em>F#</em> and <em>Elm</em> on the <em>.NET Core</em> platform.
    The source code is entirely available <a href="https://github.com/dam5s/somanyfeeds.fs">on my github</a>.
  </p>

  <p>
    You can choose to follow multiple of my feeds, on a single page, by using the menu at the top of the page.
  </p>
"""
