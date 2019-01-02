module SoManyFeeds.Read exposing (main)

import Browser exposing (Document)
import Html exposing (Html, a, div, li, p, text, ul)
import Html.Attributes exposing (href)
import SoManyFeeds.Article as Article exposing (Article)


type alias Flags =
    { userName : String
    , articles : List Article.Json
    }


type alias Model =
    { userName : String
    , articles : List Article
    }


type Msg
    = None


init : Flags -> ( Model, Cmd Msg )
init flags =
    ( { userName = flags.userName
      , articles = List.map Article.fromJson flags.articles
      }
    , Cmd.none
    )


articleView : Article -> Html Msg
articleView article =
    li [] [ text article.title ]


view : Model -> Document Msg
view model =
    { title = "SoManyFeeds - A feed aggregator by Damien Le Berrigaud"
    , body =
        [ p []
            [ text "Hello "
            , text model.userName
            , text ", your articles will show up here. But first let's "
            , a [ href "/manage" ] [ text "follow some feeds" ]
            , text "."
            ]
        , ul [] <| List.map articleView model.articles
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
