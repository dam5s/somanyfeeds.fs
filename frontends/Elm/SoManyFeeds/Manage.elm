module SoManyFeeds.Manage exposing (main)

import Browser exposing (Document)
import Html exposing (Attribute, Html, a, button, dd, div, dl, dt, form, h1, h2, h3, header, input, label, li, nav, option, p, section, select, text, ul)
import Html.Attributes exposing (class, disabled, href, selected, target, type_, value)
import Html.Events exposing (on, onInput, onSubmit, targetValue)
import Http
import Json.Decode
import Result
import SoManyFeeds.Feed as Feed exposing (Feed)


type alias Flags =
    { userName : String
    , feeds : List Feed.Json
    }


type alias Model =
    { userName : String
    , feeds : List Feed
    , form : Feed.Fields
    , formSubmitted : Bool
    }


type Msg
    = UpdateFormType String
    | UpdateFormName String
    | UpdateFormUrl String
    | CreateFeed
    | CreateFeedResult (Result Http.Error Feed.Json)


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { userName = flags.userName
      , feeds = List.map Feed.fromJson flags.feeds
      , form = Feed.emptyFields
      , formSubmitted = False
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
            [ dt [] [ text "Type" ]
            , dd [] [ text <| Feed.typeToString feed.feedType ]
            ]
        , dl []
            [ dt [] [ text "Url" ]
            , dd []
                [ a [ href feed.url, target "_blank" ] [ text feed.url ]
                ]
            ]
        , nav []
            [ button [ class "button secondary" ] [ text "Unsubscribe" ]
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
        feedType =
            model.form.feedType

        name =
            model.form.name

        url =
            model.form.url

        feedTypeOption t =
            option
                [ value <| Feed.typeToString t, selected <| feedType == t ]
                [ text <| Feed.typeToString t ]
    in
    section []
        [ form [ class "card", onSubmit CreateFeed ]
            [ h3 [] [ text "Add a feed" ]
            , label []
                [ text "Type"
                , div [ class "styled-select" ]
                    [ select [ onSelect UpdateFormType, disabled model.formSubmitted ]
                        [ feedTypeOption Feed.Rss
                        , feedTypeOption Feed.Atom
                        ]
                    ]
                ]
            , label []
                [ text "Name"
                , input [ type_ "text", value name, onInput UpdateFormName, disabled model.formSubmitted ] []
                ]
            , label []
                [ text "Url"
                , input [ type_ "text", value url, onInput UpdateFormUrl, disabled model.formSubmitted ] []
                ]
            , nav []
                [ button [ class "button primary", disabled model.formSubmitted ] [ text "Subscribe" ]
                ]
            ]
        ]


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [] [ h1 [] [ text "SoManyFeeds" ] ]
        , h2 [] [ text "Feeds" ]
        , h1 [] [ text "Manage your subscriptions" ]
        , newFeedForm model
        , feedList model
        ]
    }


feedCreated : Model -> Feed.Json -> Model
feedCreated model json =
    let
        feed =
            Feed.fromJson json
    in
    { model
        | formSubmitted = False
        , form = Feed.emptyFields
        , feeds = [ feed ] ++ model.feeds
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
        UpdateFormType value ->
            ( updateForm { form | feedType = Feed.typeFromString value }, Cmd.none )

        UpdateFormName value ->
            ( updateForm { form | name = value }, Cmd.none )

        UpdateFormUrl value ->
            ( updateForm { form | url = value }, Cmd.none )

        CreateFeed ->
            ( { model | formSubmitted = True }
            , Http.send CreateFeedResult <| Feed.createRequest form
            )

        CreateFeedResult (Result.Err err) ->
            ( { model | formSubmitted = False }, Cmd.none )

        CreateFeedResult (Result.Ok feedJson) ->
            ( feedCreated model feedJson, Cmd.none )


main : Program Flags Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = always Sub.none
        }
