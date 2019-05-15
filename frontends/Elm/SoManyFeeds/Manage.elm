module SoManyFeeds.Manage exposing (main)

import Browser exposing (Document)
import Html exposing (Attribute, Html, a, button, dd, div, dl, dt, form, h1, h2, h3, header, input, label, nav, p, section, text)
import Html.Attributes exposing (class, disabled, href, name, placeholder, target, type_, value)
import Html.Events exposing (onClick, onInput, onSubmit)
import Http
import Keyboard
import List.Extra
import Result
import SoManyFeeds.Feed as Feed exposing (Feed)
import SoManyFeeds.Logo as Logo


type alias Flags =
    { userName : String
    , maxFeeds : Int
    , feeds : List Feed
    }


type Dialog a
    = Initial
    | Opened a
    | Closed


type alias Model =
    { userName : String
    , maxFeeds : Int
    , feeds : List Feed
    , form : Feed.Fields
    , creationInProgress : Bool
    , deleteDialog : Dialog Feed
    , deletionInProgress : Bool
    }


type Msg
    = UpdateFormName String
    | UpdateFormUrl String
    | CreateFeed
    | CreateFeedResult (Result Http.Error Feed)
    | OpenDeleteDialog Feed
    | CloseDeleteDialog
    | KeyPressed Keyboard.RawKey
    | DeleteFeed
    | DeleteFeedResult Feed (Result Http.Error String)


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { userName = flags.userName
      , maxFeeds = flags.maxFeeds
      , feeds =
            flags.feeds
                |> List.sortBy .id
                |> List.reverse
      , form = Feed.emptyFields
      , creationInProgress = False
      , deleteDialog = Initial
      , deletionInProgress = False
      }
    , Cmd.none
    )


feedView : Feed -> Html Msg
feedView feed =
    div [ class "card" ]
        [ dl []
            [ dt [] [ text "Name" ]
            , dd [] [ text feed.name ]
            ]
        , dl []
            [ dt [] [ text "Url" ]
            , dd []
                [ a [ href feed.url, target "_blank" ] [ text feed.url ]
                ]
            ]
        , dl []
            [ dt [] []
            , dd [ class "actions" ]
                [ button [ class "button secondary", onClick <| OpenDeleteDialog feed ] [ text "Unsubscribe" ]
                ]
            ]
        ]


feedList : Model -> Html Msg
feedList model =
    section []
        [ div [ class "card-list" ] <|
            if List.isEmpty model.feeds then
                [ h3 [] [ text "Your feeds" ]
                , p [ class "message" ] [ text "You have not subscribed to any feeds yet." ]
                ]

            else
                [ h3 [] [ text "Your feeds" ] ]
                    ++ List.map feedView model.feeds
        ]


newFeedForm : Model -> Html Msg
newFeedForm model =
    let
        nameValue =
            model.form.name

        urlValue =
            model.form.url
    in
    section []
        [ form [ class "card", onSubmit CreateFeed ]
            [ h3 [] [ text "Add a feed" ]
            , label []
                [ text "Name"
                , input [ placeholder "Le Monde", type_ "text", name "name", value nameValue, onInput UpdateFormName, disabled model.creationInProgress ] []
                ]
            , label []
                [ text "Url"
                , input [ placeholder "https://www.lemonde.fr/rss/une.xml", type_ "text", name "url", value urlValue, onInput UpdateFormUrl, disabled model.creationInProgress ] []
                ]
            , nav []
                [ button [ class "button primary", disabled model.creationInProgress ] [ text "Subscribe" ]
                ]
            ]
        ]


overlay : Model -> Html Msg
overlay model =
    case model.deleteDialog of
        Initial ->
            div [] []

        Opened _ ->
            div [ class "overlay", onClick CloseDeleteDialog ] []

        Closed ->
            div [ class "overlay closed" ] []


deleteDialog : Model -> Html Msg
deleteDialog model =
    case model.deleteDialog of
        Opened feed ->
            div [ class "dialog" ]
                [ h3 [] [ text "Unsubscribe" ]
                , p [] [ text <| "Are you sure you want to unsubscribe from \"" ++ feed.name ++ "\"?" ]
                , nav []
                    [ button [ class "button primary", disabled model.deletionInProgress, onClick DeleteFeed ] [ text "Yes, unsubscribe" ]
                    , button [ class "button secondary", disabled model.deletionInProgress, onClick CloseDeleteDialog ] [ text "No, cancel" ]
                    ]
                ]

        _ ->
            div [] []


hasReachedMaxFeeds : Model -> Bool
hasReachedMaxFeeds model =
    List.length model.feeds >= model.maxFeeds


maxFeedsView : Model -> Html Msg
maxFeedsView model =
    section []
        [ div [ class "card" ]
            [ h3 [] [ text "Feeds limit reached" ]
            , p [ class "message" ] [ text "You have reached your feed subscription limit. You will need to unsubscribe a feed before you can create a new one." ]
            ]
        ]


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [ class "app-header" ]
            [ div []
                [ Logo.view
                , nav []
                    [ a [ href "/" ] [ text "Home" ]
                    , a [ href "/read" ] [ text "Read" ]
                    , a [ href "/manage", class "current" ] [ text "Manage" ]
                    ]
                ]
            ]
        , header [ class "page" ]
            [ div [ class "page-content" ]
                [ h2 [] [ text "Feeds" ]
                , h1 [] [ text "Manage your subscriptions" ]
                ]
            ]
        , div [ class "main" ]
            [ overlay model
            , deleteDialog model
            , if hasReachedMaxFeeds model then
                maxFeedsView model

              else
                newFeedForm model
            , feedList model
            ]
        ]
    }


feedCreated : Model -> Feed -> Model
feedCreated model feed =
    { model
        | creationInProgress = False
        , form = Feed.emptyFields
        , feeds = [ feed ] ++ model.feeds
    }


isKey : Keyboard.Key -> Keyboard.RawKey -> Bool
isKey key rawKey =
    Maybe.withDefault False <| Maybe.map ((==) key) (Keyboard.anyKey rawKey)


isEscape =
    isKey Keyboard.Escape


removeFeed : Feed -> Model -> Model
removeFeed feed model =
    { model
        | deletionInProgress = False
        , deleteDialog = Closed
        , feeds = List.Extra.filterNot (\f -> f.id == feed.id) model.feeds
    }


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    let
        form =
            model.form

        updateForm f =
            { model | form = f }
    in
    case msg of
        UpdateFormName value ->
            ( updateForm { form | name = value }, Cmd.none )

        UpdateFormUrl value ->
            ( updateForm { form | url = value }, Cmd.none )

        CreateFeed ->
            ( { model | creationInProgress = True }
            , Http.send CreateFeedResult <| Feed.createRequest form
            )

        CreateFeedResult (Result.Err _) ->
            ( { model | creationInProgress = False }, Cmd.none )

        CreateFeedResult (Result.Ok feedJson) ->
            ( feedCreated model feedJson, Cmd.none )

        OpenDeleteDialog feed ->
            ( { model | deleteDialog = Opened feed }, Cmd.none )

        CloseDeleteDialog ->
            ( { model | deleteDialog = Closed }, Cmd.none )

        KeyPressed key ->
            if isEscape key then
                ( { model | deleteDialog = Closed }, Cmd.none )

            else
                ( model, Cmd.none )

        DeleteFeed ->
            case model.deleteDialog of
                Opened feed ->
                    ( { model | deletionInProgress = True }
                    , Http.send (DeleteFeedResult feed) <| Feed.deleteRequest feed
                    )

                _ ->
                    ( model, Cmd.none )

        DeleteFeedResult feed (Ok _) ->
            ( removeFeed feed model, Cmd.none )

        DeleteFeedResult feed (Err _) ->
            ( { model | deletionInProgress = False }, Cmd.none )


subscriptions : Model -> Sub Msg
subscriptions model =
    Keyboard.ups KeyPressed


main : Program Flags Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = subscriptions
        }
