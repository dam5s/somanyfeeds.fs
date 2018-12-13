module SoManyFeeds.Manage exposing (main)

import Browser exposing (Document)
import Html exposing (Html, a, div, p, text)
import Html.Attributes exposing (href)


type alias Flags =
    { userName : String
    }


type alias Model =
    { userName : String
    }


type Msg
    = None


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { userName = flags.userName }, Cmd.none )


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ p []
            [ text "Hello "
            , text model.userName
            , text ", manage your feeds here."
            ]
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
