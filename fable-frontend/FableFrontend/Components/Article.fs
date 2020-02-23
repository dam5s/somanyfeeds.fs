module FableFrontend.Components.Article

open FableFrontend.Components.Feed
open FableFrontend.Support.Http
open Time

type Article =
    { FeedName: string
      Url: string
      Title: string
      FeedUrl: string
      Content: string
      Date: Time.Posix
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
          date: int64
          readUrl: string
          bookmarkUrl: string }

    let fromJson json =
        { FeedName = json.feedName
          Url = json.url
          Title = json.title
          FeedUrl = json.feedUrl
          Content = json.content
          Date = Posix(int64 json.date) // force to int64 here,
          // json value is not correctly parsed to that type by Fable.
          ReadUrl = json.readUrl
          BookmarkUrl = json.bookmarkUrl
          State = Unread }

    let setState newState article =
        List.map (fun a ->
            if a = article then { a with State = newState } else a)

    let listBookmarksRequest = HttpRequest.get "/api/articles/bookmarks"
    let listAllRequest = HttpRequest.get "/api/articles/recent"
    let listByFeedRequest (feed: Feed) = HttpRequest.get (sprintf "/api/articles/recent?feedId=%d" feed.Id)
    let createBookmarkRequest article = HttpRequest.post article.BookmarkUrl
    let deleteBookmarkRequest article = HttpRequest.delete article.BookmarkUrl
    let createReadArticleRequest article = HttpRequest.post article.ReadUrl
    let deleteReadArticleRequest article = HttpRequest.delete article.ReadUrl
