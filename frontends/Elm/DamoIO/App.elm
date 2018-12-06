module DamoIO.App exposing (Flags, Model, Msg(..), articlesToDisplay, init, main, sourceToggleHref, sourceView, update, view)

import Browser
import Browser.Navigation as Navigation
import DamoIO.Article as Article exposing (Article)
import DamoIO.Logo as Logo
import DamoIO.Route as Route exposing (Route(..))
import DamoIO.Source as Source exposing (Source)
import Html exposing (..)
import Html.Attributes exposing (..)
import Task
import Time
import Url


type alias Model =
    { navigationKey : Navigation.Key
    , route : Route
    , articles : List Article
    , timeZone : Maybe Time.Zone
    }


type alias Flags =
    { articles : List Article.Json
    }


init : Flags -> Url.Url -> Navigation.Key -> ( Model, Cmd Msg )
init flags url navigationKey =
    ( { navigationKey = navigationKey
      , route = Route.fromUrl url
      , articles = [ Article.about ] ++ List.map Article.fromJson flags.articles
      , timeZone = Nothing
      }
    , Task.perform UpdateTimeZone Time.here
    )


type Msg
    = OnUrlRequest Browser.UrlRequest
    | OnUrlChange Url.Url
    | UpdateTimeZone Time.Zone


view : Model -> Browser.Document Msg
view model =
    { title = "damo.io - Damien Le Berrigaud's Feed Aggregator"
    , body =
        [ header [ id "app-header" ]
            [ section [ class "content" ]
                [ Logo.view
                , aside [ id "app-menu" ]
                    [ ul [] <| List.map (sourceView model) Source.all ]
                ]
            ]
        , Logo.view
        , section [ id "app-content", class "content" ] <|
            List.map (Article.view model.timeZone) (articlesToDisplay model)
        ]
    }


articlesToDisplay : Model -> List Article
articlesToDisplay model =
    let
        sources =
            Route.sources model.route
    in
    if List.isEmpty sources then
        [ Article.default ]

    else
        model.articles
            |> List.filter (\a -> List.member a.source sources)


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
        OnUrlRequest urlRequest ->
            case urlRequest of
                Browser.Internal url ->
                    ( model
                    , Navigation.pushUrl model.navigationKey (Url.toString url)
                    )

                Browser.External url ->
                    ( model
                    , Navigation.load url
                    )

        OnUrlChange url ->
            ( { model | route = Route.fromUrl url }, Cmd.none )

        UpdateTimeZone timeZone ->
            ( { model | timeZone = Just timeZone }, Cmd.none )


main : Program Flags Model Msg
main =
    Browser.application
        { init = init
        , view = view
        , update = update
        , subscriptions = \_ -> Sub.none
        , onUrlRequest = OnUrlRequest
        , onUrlChange = OnUrlChange
        }
