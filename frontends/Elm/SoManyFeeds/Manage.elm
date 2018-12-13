module SoManyFeeds.Manage exposing (main)

import Browser exposing (Document)
import Html exposing (Html, a, div, li, p, text, ul)
import Html.Attributes exposing (href)
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
    li [] [ text feed.name ]


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ p []
            [ text "Hello "
            , text model.userName
            , text ", manage your feeds here."
            ]
        , ul [] <| List.map feedView model.feeds
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
