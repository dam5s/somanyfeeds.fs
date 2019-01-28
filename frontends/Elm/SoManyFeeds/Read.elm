module SoManyFeeds.Read exposing (main)

import Browser exposing (Document)
import Html exposing (Html, a, article, button, div, h1, h2, h3, h4, header, nav, p, section, text)
import Html.Attributes exposing (class, href, target, type_)
import Html.Events exposing (onClick)
import Http
import SoManyFeeds.Article as Article exposing (Article)
import SoManyFeeds.Logo as Logo
import Support.DateFormat as DateFormat
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
    | MarkRead Article
    | MarkReadResult Article (Result Http.Error String)


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { userName = flags.userName
      , articles = List.map Article.fromJson flags.articles
      , timeZone = Nothing
      }
    , Task.perform UpdateTimeZone Time.here
    )


articleView : Model -> Article -> Html Msg
articleView model record =
    article [ class "card" ]
        [ h4 [] [ text record.feedName ]
        , h3 []
            [ a [ href record.url, target "_blank" ] <| RawHtml.fromString record.title ]
        , p [ class "date" ] [ text <| DateFormat.tryFormat model.timeZone record.date ]
        , div [ class "content" ] <| RawHtml.fromString record.content
        , nav []
            [ button [ onClick (MarkRead record), type_ "button", class "button secondary mark-read" ] [ text "Mark read" ]
            ]
        ]


articleList : Model -> Html Msg
articleList model =
    section [] <| List.map (articleView model) model.articles


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [ class "app-header" ]
            [ div []
                [ Logo.view
                , nav []
                    [ a [ href "/read", class "current" ] [ text "Read" ]
                    , a [ href "/manage" ] [ text "Manage" ]
                    ]
                ]
            ]
        , header [ class "page" ]
            [ h2 [] [ text "Articles" ]
            , h1 [] [ text "Most recent" ]
            ]
        , div [ class "main" ] [ articleList model ]
        ]
    }


removeArticle : Article -> Model -> Model
removeArticle record model =
    { model
        | articles =
            List.filter (\a -> a /= record) model.articles
    }


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        UpdateTimeZone timeZone ->
            ( { model | timeZone = Just timeZone }, Cmd.none )

        MarkRead record ->
            ( model
            , Http.send (MarkReadResult record) (Article.markReadRequest record)
            )

        MarkReadResult record (Ok _) ->
            ( removeArticle record model, Cmd.none )

        MarkReadResult record (Err _) ->
            ( model, Cmd.none )


main : Program Flags Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = always Sub.none
        }
