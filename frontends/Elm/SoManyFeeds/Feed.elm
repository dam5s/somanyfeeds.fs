module SoManyFeeds.Feed exposing (Feed, FeedType, Json, typeToString, fromJson)


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
