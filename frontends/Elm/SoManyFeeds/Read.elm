module SoManyFeeds.Read exposing (main)

import Browser exposing (Document, UrlRequest)
import Browser.Navigation as Nav exposing (Key)
import Html exposing (Attribute, Html, a, article, button, div, h1, h2, h3, h4, header, nav, option, p, section, select, text)
import Html.Attributes exposing (class, href, selected, target, type_, value)
import Html.Events exposing (on, onClick, targetValue)
import Http
import Json.Decode
import SoManyFeeds.Article as Article exposing (Article)
import SoManyFeeds.Feed exposing (Feed)
import SoManyFeeds.Logo as Logo
import SoManyFeeds.RemoteData as RemoteData exposing (RemoteData(..))
import Support.DateFormat as DateFormat
import Support.RawHtml as RawHtml
import Task
import Time
import Url exposing (Url)


type alias Flags =
    { userName : String
    , articles : List Article.Json
    , feeds : List Feed
    , selectedFeedId : Maybe Int
    }


type alias Model =
    { navKey : Key
    , userName : String
    , articles : RemoteData (List Article)
    , feeds : List Feed
    , selectedFeedId : Maybe Int
    , timeZone : Maybe Time.Zone
    }


type Msg
    = ClickedLink UrlRequest
    | ChangedUrl Url
    | FilterByFeed String
    | ReceivedArticles (Result Http.Error (List Article.Json))
    | UpdateTimeZone Time.Zone
    | MarkRead Article
    | MarkReadResult Article (Result Http.Error String)
    | MarkUnread Article
    | MarkUnreadResult Article (Result Http.Error String)


init : Flags -> Url -> Key -> ( Model, Cmd Msg )
init flags _ navKey =
    ( { navKey = navKey
      , userName = flags.userName
      , articles = Loaded (List.map Article.fromJson flags.articles)
      , feeds = flags.feeds
      , selectedFeedId = flags.selectedFeedId
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
    case model.articles of
        Loaded [] ->
            if List.isEmpty model.feeds then
                section []
                    [ div [ class "card" ]
                        [ p [ class "message" ] [ text "You have not ", a [ href "/manage" ] [ text "subscribed to any feed" ], text " yet." ]
                        , p [ class "message" ] [ text "Please use the ", a [ href "/manage" ] [ text "manage tab" ], text " to subscribe to some feeds." ]
                        ]
                    ]

            else
                section []
                    [ div [ class "card" ]
                        [ p [ class "message" ] [ text "No unread articles." ]
                        , p [ class "message" ] [ text "New feed subscriptions may take ~10 minutes before being available." ]
                        ]
                    ]

        Loaded articles ->
            section [] <| List.map (articleView model) articles

        Loading ->
            section []
                [ div [ class "card" ]
                    [ p [ class "message" ] [ text "Loading your reading list. Thank you for your patience." ]
                    ]
                ]

        Error message ->
            section []
                [ div [ class "card" ]
                    [ p [ class "message" ] [ text "There was an error while loading your articles." ]
                    , p [ class "message" ] [ text message ]
                    ]
                ]


feedOptions : Model -> List (Html Msg)
feedOptions model =
    let
        feedOption feed =
            option [ selected (model.selectedFeedId == Just feed.id), value (String.fromInt feed.id) ] [ text ("Show only " ++ feed.name) ]
    in
    [ option [ selected (model.selectedFeedId == Nothing), value "" ] [ text "Show all subscriptions" ] ]
        ++ List.map feedOption model.feeds


{-| This replaces onInput for selects.

onInput only works in Chrome for selects,
other browsers trigger a change event.

See: <https://github.com/elm-lang/html/issues/71>

-}
onSelect : (String -> msg) -> Attribute msg
onSelect msg =
    on "change" (Json.Decode.map msg targetValue)


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [ class "app-header" ]
            [ div []
                [ Logo.view
                , nav []
                    [ a [ href "/" ] [ text "Home" ]
                    , a [ href "/read", class "current" ] [ text "Read" ]
                    , a [ href "/manage" ] [ text "Manage" ]
                    ]
                ]
            ]
        , header [ class "page" ]
            [ h2 [] [ text "Articles" ]
            , h1 [] [ text "Your reading list" ]
            , nav []
                [ div [ class "styled-select" ]
                    [ select [ onSelect FilterByFeed ] (feedOptions model)
                    ]
                ]
            ]
        , div [ class "main" ] [ articleList model ]
        ]
    }


loadAllFeeds : Cmd Msg
loadAllFeeds =
    Http.send ReceivedArticles Article.listAllRequest


loadFeed : String -> Cmd Msg
loadFeed feedId =
    Http.send ReceivedArticles (Article.listByFeedRequest feedId)


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        ClickedLink (Browser.Internal url) ->
            ( model, Nav.load (Url.toString url) )

        ClickedLink (Browser.External url) ->
            ( model, Nav.load url )

        ChangedUrl _ ->
            ( model, Cmd.none )

        FilterByFeed "" ->
            ( { model | articles = Loading }, Cmd.batch [ Nav.pushUrl model.navKey "/read", loadAllFeeds ] )

        FilterByFeed feedId ->
            let
                feedUrl =
                    "/read/feed/" ++ feedId
            in
            ( { model | articles = Loading }, Cmd.batch [ Nav.pushUrl model.navKey feedUrl, loadFeed feedId ] )

        UpdateTimeZone timeZone ->
            ( { model | timeZone = Just timeZone }, Cmd.none )

        MarkRead record ->
            ( model
            , Http.send (MarkReadResult record) (Article.markReadRequest record)
            )

        MarkReadResult record (Ok _) ->
            let
                updatedArticles =
                    model.articles
                        |> RemoteData.map (Article.setState Article.Read record)
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
                    model.articles
                        |> RemoteData.map (Article.setState Article.Unread record)
            in
            ( { model | articles = updatedArticles }, Cmd.none )

        MarkUnreadResult record (Err _) ->
            ( model, Cmd.none )

        ReceivedArticles (Ok articles) ->
            ( { model | articles = Loaded (List.map Article.fromJson articles) }, Cmd.none )

        ReceivedArticles (Err err) ->
            ( { model | articles = RemoteData.errorFromHttp err }, Cmd.none )


main : Program Flags Model Msg
main =
    Browser.application
        { init = init
        , view = view
        , update = update
        , subscriptions = always Sub.none
        , onUrlRequest = ClickedLink
        , onUrlChange = ChangedUrl
        }
