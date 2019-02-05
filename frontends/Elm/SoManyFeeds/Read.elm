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
    | MarkUnread Article
    | MarkUnreadResult Article (Result Http.Error String)


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
    case record.state of
        Article.Unread ->
            article [ class "card" ]
                [ div [ class "row" ]
                    [ header []
                        [ h4 [] [ text record.feedName ]
                        , h3 [] [ a [ href record.url, target "_blank" ] <| RawHtml.parseEntities record.title ]
                        ]
                    , button [ onClick (MarkRead record), type_ "button", class "flex-init button icon-only mark-read" ] [ text "Mark read" ]
                    ]
                , p [ class "date" ] [ text <| DateFormat.tryFormat model.timeZone record.date ]
                , div [ class "content" ] <| RawHtml.fromString record.content
                ]

        Article.Read ->
            article [ class "card row read" ]
                [ h3 [] <| RawHtml.parseEntities record.title
                , button [ onClick (MarkUnread record), type_ "button", class "button icon-only undo flex-init" ] [ text "Undo" ]
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
            , h1 [] [ text "Your reading list" ]
            ]
        , div [ class "main" ] [ articleList model ]
        ]
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
            let
                updatedArticles =
                    Article.setState Article.Read record model.articles
            in
            ( { model | articles = updatedArticles }, Cmd.none )

        MarkReadResult record (Err _) ->
            ( model, Cmd.none )

        MarkUnread record ->
            ( model
            , Http.send (MarkUnreadResult record) (Article.markUnreadRequest record)
            )

        MarkUnreadResult record (Ok _) ->
            let
                updatedArticles =
                    Article.setState Article.Unread record model.articles
            in
            ( { model | articles = updatedArticles }, Cmd.none )

        MarkUnreadResult record (Err _) ->
            ( model, Cmd.none )


main : Program Flags Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = always Sub.none
        }
