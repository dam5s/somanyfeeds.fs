module SoManyFeeds.Manage exposing (main)

import Browser exposing (Document)
import Html exposing (Html, a, button, dd, div, dl, dt, form, h1, h2, h3, header, input, label, li, nav, p, section, text, ul)
import Html.Attributes exposing (class, href, type_)
import SoManyFeeds.Feed as Feed exposing (Feed)


type alias Flags =
    { userName : String
    , feeds : List Feed.Json
    }


type alias Model =
    { userName : String
    , feeds : List Feed
    }


type Msg
    = None


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { userName = flags.userName
      , feeds = List.map Feed.fromJson flags.feeds
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
            , dd [] [ text feed.url ]
            ]
        , nav []
            [ button [ class "button secondary" ] [ text "Delete" ]
            ]
        ]


feedList : Model -> Html Msg
feedList model =
    section []
        [ div [ class "card-list" ] <|
            [ h3 [] [ text "Your feeds" ] ]
                ++ List.map feedView model.feeds
        ]


newFeedForm =
    section []
        [ form [ class "card" ]
            [ label []
                [ text "Type"
                , input [ type_ "text" ] []
                ]
            , label []
                [ text "Name"
                , input [ type_ "text" ] []
                ]
            , label []
                [ text "Url"
                , input [ type_ "text" ] []
                ]
            , nav []
                [ button [ class "button primary" ] [ text "Create" ]
                ]
            ]
        ]


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ header [] [ h1 [] [ text "SoManyFeeds" ] ]
        , h2 [] [ text "Manage" ]
        , h1 [] [ text "Feeds" ]
        , newFeedForm
        , feedList model
        ]
    }


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        None ->
            ( model, Cmd.none )


main : Program Flags Model Msg
main =
    Browser.document
        { init = init
        , view = view
        , update = update
        , subscriptions = always Sub.none
        }
