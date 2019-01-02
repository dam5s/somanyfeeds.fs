module SoManyFeeds.Read exposing (main)

import Browser exposing (Document)
import Html exposing (Html, a, article, div, h1, h2, h3, header, li, p, section, text, ul)
import Html.Attributes exposing (class, href)
import SoManyFeeds.Article as Article exposing (Article)
import Support.RawHtml as RawHtml
import Task
import Time


type alias Flags =
    { userName : String
    , articles : List Article.Json
    }


type alias Model =
    { userName : String
    , articles : List Article
    , timeZone : Maybe Time.Zone
    }


type Msg
    = UpdateTimeZone Time.Zone


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { userName = flags.userName
      , articles = List.map Article.fromJson flags.articles
      , timeZone = Nothing
      }
    , Task.perform UpdateTimeZone Time.here
    )


articleView : Article -> Html Msg
articleView record =
    section []
        [ article [ class "card" ]
            [ h3 []
                [ a [ href record.url ] [ text record.title ]
                ]
            , div [ class "content" ] <| RawHtml.fromString record.content
            ]
        ]


articleList : Model -> Html Msg
articleList model =
    div [] <| List.map articleView model.articles


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [] [ h1 [] [ text "SoManyFeeds" ] ]
        , h2 [] [ text "Articles" ]
        , h1 [] [ text "Most recent" ]
        , articleList model
        ]
    }


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        UpdateTimeZone timeZone ->
            ( { model | timeZone = Just timeZone }, Cmd.none )


main : Program Flags Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = always Sub.none
        }
