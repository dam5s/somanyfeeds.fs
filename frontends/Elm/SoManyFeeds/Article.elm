module SoManyFeeds.Article exposing (Article, Json, fromJson, markReadRequest)

import Http
import Time


type alias Article =
    { feedName : String
    , url : String
    , title : String
    , feedUrl : String
    , content : String
    , date : Time.Posix
    , markReadUrl : String
    }


type alias Json =
    { feedName : String
    , url : String
    , title : String
    , feedUrl : String
    , content : String
    , date : Int
    , markReadUrl : String
    }


fromJson : Json -> Article
fromJson json =
    { feedName = json.feedName
    , url = json.url
    , title = json.title
    , feedUrl = json.feedUrl
    , content = json.content
    , date = Time.millisToPosix json.date
    , markReadUrl = json.markReadUrl
    }


markReadRequest : Article -> Http.Request String
markReadRequest article =
    Http.request
        { method = "PUT"
        , headers = []
        , url = article.markReadUrl
        , body = Http.emptyBody
        , expect = Http.expectString
        , timeout = Nothing
        , withCredentials = False
        }
