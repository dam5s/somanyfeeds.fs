module SoManyFeeds.Article exposing (Article, Json, fromJson)

import Time


type alias Article =
    { feedName : String
    , url : String
    , title : String
    , feedUrl : String
    , content : String
    , date : Time.Posix
    }


type alias Json =
    { feedName : String
    , url : String
    , title : String
    , feedUrl : String
    , content : String
    , date : Int
    }


fromJson : Json -> Article
fromJson json =
    { feedName = json.feedName
    , url = json.url
    , title = json.title
    , feedUrl = json.feedUrl
    , content = json.content
    , date = Time.millisToPosix json.date
    }
