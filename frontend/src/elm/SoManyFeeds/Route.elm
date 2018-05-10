module SoManyFeeds.Route exposing (Route(..), fromLocation, sources)

import Navigation
import SoManyFeeds.Source as Source exposing (Source)
import UrlParser exposing (..)


type Route
    = NoSourcesSelectedRoute
    | SelectedSourcesRoute (List Source)
    | NotFoundRoute


sources : Route -> List Source
sources route =
    case route of
        NoSourcesSelectedRoute ->
            []

        SelectedSourcesRoute sources ->
            sources

        NotFoundRoute ->
            []


matchers : Parser (Route -> a) a
matchers =
    oneOf
        [ map NoSourcesSelectedRoute (s "")
        , map (SelectedSourcesRoute << extractSources) string
        ]


extractSources : String -> List Source
extractSources hash =
    String.split "," hash |> List.map Source.fromString


fromLocation : Navigation.Location -> Route
fromLocation location =
    parseHash matchers location |> Maybe.withDefault NotFoundRoute
