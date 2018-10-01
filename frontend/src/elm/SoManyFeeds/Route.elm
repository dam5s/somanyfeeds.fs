module SoManyFeeds.Route exposing (Route(..), fromUrl, sources)

import SoManyFeeds.Source as Source exposing (Source)
import Url
import Url.Parser exposing (..)


type Route
    = NoSourcesSelectedRoute
    | SelectedSourcesRoute (List Source)
    | NotFoundRoute


sources : Route -> List Source
sources route =
    case route of
        NoSourcesSelectedRoute ->
            []

        SelectedSourcesRoute s ->
            s

        NotFoundRoute ->
            []


extractSources : String -> List Source
extractSources hash =
    String.split "," hash |> List.map Source.fromString


fromUrl : Url.Url -> Route
fromUrl url =
    let
        selectedSources =
            url.fragment
                |> Maybe.withDefault ""
                |> extractSources
    in
    case selectedSources of
        [] ->
            NoSourcesSelectedRoute

        s ->
            SelectedSourcesRoute s
