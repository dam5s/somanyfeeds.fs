module SoManyFeeds.Article exposing (Article, Json, State(..), bookmarkRequest, fromJson, listAllRequest, listBookmarksRequest, listByFeedRequest, readRequest, removeBookmarkRequest, setState, unreadRequest)

import Http
import Json.Decode
import SoManyFeeds.Feed exposing (Feed)
import Time


type State
    = Unread
    | Read
    | Bookmarked


type alias Article =
    { feedName : String
    , url : String
    , title : String
    , feedUrl : String
    , content : String
    , date : Time.Posix
    , readUrl : String
    , bookmarkUrl : String
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
    , bookmarkUrl : String
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
    , bookmarkUrl = json.bookmarkUrl
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


simplePostRequest url =
    simpleRequest { method = "POST", url = url }


simpleDeleteRequest url =
    simpleRequest { method = "DELETE", url = url }


readRequest article =
    simplePostRequest article.readUrl


unreadRequest article =
    simpleDeleteRequest article.readUrl


bookmarkRequest article =
    simplePostRequest article.bookmarkUrl


removeBookmarkRequest article =
    simpleDeleteRequest article.bookmarkUrl


decoder : Json.Decode.Decoder Json
decoder =
    Json.Decode.map8 Json
        (Json.Decode.field "feedName" Json.Decode.string)
        (Json.Decode.field "url" Json.Decode.string)
        (Json.Decode.field "title" Json.Decode.string)
        (Json.Decode.field "feedUrl" Json.Decode.string)
        (Json.Decode.field "content" Json.Decode.string)
        (Json.Decode.field "date" Json.Decode.int)
        (Json.Decode.field "readUrl" Json.Decode.string)
        (Json.Decode.field "bookmarkUrl" Json.Decode.string)


listAllRequest : Http.Request (List Json)
listAllRequest =
    Http.get "/api/articles/recent" (Json.Decode.list decoder)


listBookmarksRequest : Http.Request (List Json)
listBookmarksRequest =
    Http.get "/api/articles/bookmarks" (Json.Decode.list decoder)


listByFeedRequest : Feed -> Http.Request (List Json)
listByFeedRequest feed =
    let
        path =
            "/api/articles/recent?feedId=" ++ String.fromInt feed.id
    in
    Http.get path (Json.Decode.list decoder)
