module SoManyFeeds.Article exposing (Article, Json, State(..), fromJson, markReadRequest, markUnreadRequest, setState)

import Http
import Time


type State
    = Unread
    | Read


type alias Article =
    { feedName : String
    , url : String
    , title : String
    , feedUrl : String
    , content : String
    , date : Time.Posix
    , readUrl : String
    , state : State
    }


type alias Json =
    { feedName : String
    , url : String
    , title : String
    , feedUrl : String
    , content : String
    , date : Int
    , readUrl : String
    }


fromJson : Json -> Article
fromJson json =
    { feedName = json.feedName
    , url = json.url
    , title = json.title
    , feedUrl = json.feedUrl
    , content = json.content
    , date = Time.millisToPosix json.date
    , readUrl = json.readUrl
    , state = Unread
    }


setState : State -> Article -> List Article -> List Article
setState newState article =
    List.map
        (\a ->
            if a == article then
                { a | state = newState }

            else
                a
        )


simpleRequest : { method : String, url : String } -> Http.Request String
simpleRequest options =
    Http.request
        { method = options.method
        , headers = []
        , url = options.url
        , body = Http.emptyBody
        , expect = Http.expectString
        , timeout = Nothing
        , withCredentials = False
        }


markReadRequest : Article -> Http.Request String
markReadRequest article =
    simpleRequest
        { method = "POST"
        , url = article.readUrl
        }


markUnreadRequest : Article -> Http.Request String
markUnreadRequest article =
    simpleRequest
        { method = "DELETE"
        , url = article.readUrl
        }
