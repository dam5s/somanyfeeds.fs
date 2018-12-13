module SoManyFeeds.Feed exposing (Feed, FeedType, Json, fromJson)


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
    , feedType = feedTypeFromString json.feedType
    , name = json.name
    , url = json.url
    }


feedTypeFromString : String -> FeedType
feedTypeFromString value =
    case value of
        "Atom" ->
            Atom

        _ ->
            Rss
