module SoManyFeeds.RemoteData exposing (RemoteData(..), errorFromHttp, map)

import Http


type RemoteData a
    = NotLoaded
    | Loading
    | Loaded a
    | Error String


serverErr =
    Error "Server error, please try again later"


networkErr =
    Error "Networking error, please check your connection"


errorFromHttp : Http.Error -> RemoteData a
errorFromHttp err =
    case err of
        Http.Timeout ->
            networkErr

        Http.NetworkError ->
            networkErr

        _ ->
            serverErr


map : (a -> b) -> RemoteData a -> RemoteData b
map function remoteData =
    case remoteData of
        Loaded data ->
            Loaded (function data)

        NotLoaded ->
            NotLoaded

        Loading ->
            Loading

        Error msg ->
            Error msg
