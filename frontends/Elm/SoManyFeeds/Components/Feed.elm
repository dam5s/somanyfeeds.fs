module SoManyFeeds.Components.Feed exposing (Feed, Fields, createRequest, deleteRequest, emptyFields)

import Http
import Json.Decode as Decode
import Json.Encode as Encode


type alias Feed =
    { id : Int
    , name : String
    , url : String
    }


type alias Fields =
    { name : String
    , url : String
    }


emptyFields : Fields
emptyFields =
    { name = ""
    , url = ""
    }


decoder : Decode.Decoder Feed
decoder =
    Decode.map3
        Feed
        (Decode.field "id" Decode.int)
        (Decode.field "name" Decode.string)
        (Decode.field "url" Decode.string)


fieldsEncoder : Fields -> Encode.Value
fieldsEncoder fields =
    Encode.object
        [ ( "name", Encode.string fields.name )
        , ( "url", Encode.string fields.url )
        ]


createRequest : Fields -> Http.Request Feed
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
