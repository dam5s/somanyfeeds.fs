module SoManyFeeds.App exposing (..)

import Html exposing (..)
import Html.Attributes exposing (..)
import Navigation
import SoManyFeeds.Article as Article exposing (Article)
import SoManyFeeds.Logo as Logo
import SoManyFeeds.Route as Route exposing (Route(..))
import SoManyFeeds.Source as Source exposing (Source)


type alias Model =
    { route : Route
    , articles : List Article
    }


type alias Flags =
    { articles : List Article.Json
    }


init : Flags -> Navigation.Location -> ( Model, Cmd Msg )
init flags location =
    { route = Route.fromLocation location
    , articles = List.map Article.fromJson flags.articles
    }
        ! []


type Msg
    = UrlChange Navigation.Location


view : Model -> Html Msg
view model =
    div []
        [ header [ id "app-header" ]
            [ section [ class "content" ]
                [ Logo.view
                , aside [ id "app-menu" ]
                    [ ul [] <| List.map (sourceView model) Source.all ]
                ]
            ]
        , Logo.view
        , section [ id "app-content", class "content" ] <|
            List.map Article.view (articlesToDisplay model)
        ]


articlesToDisplay : Model -> List Article
articlesToDisplay model =
    model.articles


sourceView : Model -> Source -> Html Msg
sourceView model source =
    let
        selectedSources =
            Route.sources model.route

        selectedClass =
            if List.member source selectedSources then
                "selected"
            else
                ""
    in
    li [ class selectedClass ]
        [ a [ href <| sourceToggleHref selectedSources source ]
            [ text <| Source.toString source
            ]
        ]


sourceToggleHref : List Source -> Source -> String
sourceToggleHref selectedSources source =
    "#"
        ++ (Source.all
                |> List.filterMap
                    (\s ->
                        if s == source then
                            if List.member s selectedSources then
                                Nothing
                            else
                                Just <| Source.toString s
                        else if List.member s selectedSources then
                            Just <| Source.toString s
                        else
                            Nothing
                    )
                |> String.join ","
           )


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        UrlChange location ->
            { model | route = Route.fromLocation location } ! []


main : Program Flags Model Msg
main =
    Navigation.programWithFlags UrlChange
        { init = init
        , view = view
        , update = update
        , subscriptions = \_ -> Sub.none
        }
