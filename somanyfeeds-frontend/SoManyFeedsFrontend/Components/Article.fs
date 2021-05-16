module SoManyFeedsFrontend.Components.Article

open Fable.Core
open SoManyFeedsFrontend.Support
open SoManyFeedsFrontend.Support.Http
open Result.Operators
open Time

type Article =
    { FeedName: string
      Url: string
      Title: string
      FeedUrl: string
      Content: string
      Date: Time.Posix option
      ReadUrl: string
      BookmarkUrl: string
      State: ArticleState }

and ArticleState =
    | Unread
    | Read
    | Bookmarked

[<RequireQualifiedAccess>]
module Article =
    type Json =
        { feedName: string
          url: string
          title: string
          feedUrl: string
          content: string
          date: int64 option
          readUrl: string
          bookmarkUrl: string }

    let fromJson json =
        { FeedName = json.feedName
          Url = json.url
          Title = json.title
          FeedUrl = json.feedUrl
          Content = json.content
          Date = json.date |> Option.map (int64 >> Posix) // force to int64 here,
          // json value is not correctly parsed to that type by Fable.
          ReadUrl = json.readUrl
          BookmarkUrl = json.bookmarkUrl
          State = Unread }

    let decoder (obj: JS.Object) =
        let constructor a b c d e f g h =
            { feedName = a; url = b; title = c; feedUrl = d; content = e; date = f; readUrl = g; bookmarkUrl = h }

        let result =
            constructor
            <!> (Json.property "feedName" obj)
            <*> (Json.property "url" obj)
            <*> (Json.property "title" obj)
            <*> (Json.property "feedUrl" obj)
            <*> (Json.property "content" obj)
            <*> (Json.property "date" obj)
            <*> (Json.property "readUrl" obj)
            <*> (Json.property "bookmarkUrl" obj)

        Result.map fromJson result

    let setState newState article =
        List.map (fun a ->
            if a = article then { a with State = newState } else a)

    let listBookmarksRequest = HttpRequest.get "/api/articles/bookmarks"
    let listAllRequest = HttpRequest.get "/api/articles/recent"
    let listByFeedRequest (feedId: int64) = HttpRequest.get $"/api/articles/recent?feedId=%d{feedId}"
    let createBookmarkRequest article = HttpRequest.post article.BookmarkUrl
    let deleteBookmarkRequest article = HttpRequest.delete article.BookmarkUrl
    let createReadArticleRequest article = HttpRequest.post article.ReadUrl
    let deleteReadArticleRequest article = HttpRequest.delete article.ReadUrl
