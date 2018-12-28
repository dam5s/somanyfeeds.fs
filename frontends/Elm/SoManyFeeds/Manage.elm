module SoManyFeeds.Manage exposing (main)

import Browser exposing (Document)
import Html exposing (Attribute, Html, a, button, dd, div, dl, dt, form, h1, h2, h3, header, input, label, li, nav, option, p, section, select, text, ul)
import Html.Attributes exposing (class, disabled, href, selected, target, type_, value)
import Html.Events exposing (on, onClick, onInput, onSubmit, targetValue)
import Http
import Json.Decode
import Keyboard
import List.Extra
import Result
import SoManyFeeds.Feed as Feed exposing (Feed)


type alias Flags =
    { userName : String
    , feeds : List Feed
    }


type Dialog a
    = Initial
    | Opened a
    | Closed


type alias Model =
    { userName : String
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
        , nav []
            [ button [ class "button secondary", onClick <| OpenDeleteDialog feed ] [ text "Unsubscribe" ]
            ]
        ]


feedList : Model -> Html Msg
feedList model =
    section []
        [ div [ class "card-list" ] <|
            [ h3 [] [ text "Your feeds" ] ]
                ++ List.map feedView model.feeds
        ]


onSelect : (String -> msg) -> Attribute msg
onSelect msg =
    on "change" (Json.Decode.map msg targetValue)


newFeedForm : Model -> Html Msg
newFeedForm model =
    let
        name =
            model.form.name

        url =
            model.form.url
    in
    section []
        [ form [ class "card", onSubmit CreateFeed ]
            [ h3 [] [ text "Add a feed" ]
            , label []
                [ text "Name"
                , input [ type_ "text", value name, onInput UpdateFormName, disabled model.creationInProgress ] []
                ]
            , label []
                [ text "Url"
                , input [ type_ "text", value url, onInput UpdateFormUrl, disabled model.creationInProgress ] []
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


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [] [ h1 [] [ text "SoManyFeeds" ] ]
        , h2 [] [ text "Feeds" ]
        , h1 [] [ text "Manage your subscriptions" ]
        , newFeedForm model
        , feedList model
        , overlay model
        , deleteDialog model
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

        CreateFeedResult (Result.Err err) ->
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
