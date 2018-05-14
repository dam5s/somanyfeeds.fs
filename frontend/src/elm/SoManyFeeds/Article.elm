module SoManyFeeds.Article exposing (Article, Json, fromJson, view)

import Html exposing (Attribute, Html, a, h1, h2, header, section, text)
import Html.Attributes exposing (class, href)
import Json.Encode
import Regex
import SoManyFeeds.DateFormat as DateFormat
import SoManyFeeds.Source as Source exposing (Source)
import SoManyFeeds.Tweet as Tweet
import VirtualDom


type alias Json =
    { title : Maybe String
    , link : Maybe String
    , content : String
    , date : Maybe String
    , source : String
    }


type alias Article =
    { title : Maybe String
    , link : Maybe String
    , content : String
    , date : Maybe String
    , source : Source
    }


fromJson : Json -> Article
fromJson json =
    { title = json.title
    , link = json.link
    , content = json.content
    , date = json.date
    , source = Source.fromString json.source
    }


view : Article -> Html msg
view article =
    case article.title of
        Just t ->
            Html.article []
                [ articleHeader article
                , section [ innerHtml article.content ] []
                ]

        Nothing ->
            Html.article [ class "tweet" ]
                [ articleHeader article
                , Tweet.display article.content
                ]


articleHeader : Article -> Html msg
articleHeader article =
    header []
        (compact
            [ Maybe.map (articleTitle article) article.title
            , Maybe.map articleDate article.date
            ]
        )


articleDate : String -> Html msg
articleDate date =
    h2 [ class "date" ] [ text (DateFormat.parseAndFormat date) ]


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
            Regex.regex "\\shttp(s?)://[^\\s]*"
    in
    Regex.replace Regex.All regex (\_ -> "") title


compact : List (Maybe a) -> List a
compact list =
    List.filterMap identity list


innerHtml : String -> Attribute msg
innerHtml =
    VirtualDom.property "innerHTML" << Json.Encode.string
