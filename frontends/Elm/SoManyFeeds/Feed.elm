module SoManyFeeds.Feed exposing (Feed, FeedType(..), Fields, Json, createRequest, deleteRequest, emptyFields, fromJson, typeFromString, typeToString)

import Http
import Json.Decode as Decode
import Json.Encode as Encode


type alias Json =
    { id : Int
    , feedType : String
    , name : String
    , url : String
    }


type FeedType
    = Rss
    | Atom


type alias Feed =
    { id : Int
    , feedType : FeedType
    , name : String
    , url : String
    }


type alias Fields =
    { feedType : FeedType
    , name : String
    , url : String
    }


fromJson : Json -> Feed
fromJson json =
    { id = json.id
    , feedType = typeFromString json.feedType
    , name = json.name
    , url = json.url
    }


typeFromString : String -> FeedType
typeFromString value =
    case value of
        "Atom" ->
            Atom

        _ ->
            Rss


typeToString : FeedType -> String
typeToString value =
    case value of
        Atom ->
            "Atom"

        Rss ->
            "Rss"


emptyFields : Fields
emptyFields =
    { feedType = Rss
    , name = ""
    , url = ""
    }


decoder : Decode.Decoder Json
decoder =
    Decode.map4
        Json
        (Decode.field "id" Decode.int)
        (Decode.field "feedType" Decode.string)
        (Decode.field "name" Decode.string)
        (Decode.field "url" Decode.string)


fieldsEncoder : Fields -> Encode.Value
fieldsEncoder fields =
    Encode.object
        [ ( "feedType", Encode.string <| typeToString fields.feedType )
        , ( "name", Encode.string fields.name )
        , ( "url", Encode.string fields.url )
        ]


createRequest : Fields -> Http.Request Json
createRequest fields =
    let
        body =
            Http.jsonBody <| fieldsEncoder fields
    in
    Http.post "/api/feeds" body decoder


deleteRequest : Feed -> Http.Request String
deleteRequest feed =
    Http.request
        { method = "DELETE"
        , headers = []
        , url = "/api/feeds/" ++ String.fromInt feed.id
        , body = Http.emptyBody
        , expect = Http.expectString
        , timeout = Nothing
        , withCredentials = False
        }
