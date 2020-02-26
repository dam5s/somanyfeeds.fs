[<RequireQualifiedAccess>]
module SoManyFeedsServer.WebApp

open System
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open SoManyFeedsDomain.User
open SoManyFeedsPersistence
open SoManyFeedsPersistence.UserArticlesDataGateway
open SoManyFeedsServer

let private maxFeeds =
    Env.var "MAX_FEEDS"
    |> Option.map int
    |> Option.defaultValue 20


let private paramToId param =
    unsafeOperation "Reading id query param" { return fun _ -> int64 param } |> Result.toOption


type private UserFeeds(user: User) =
    member this.List = FeedsDataGateway.listFeeds user.Id
    member this.Create = FeedsService.createFeed maxFeeds user.Id
    member this.Update = FeedsDataGateway.updateFeed user.Id
    member this.Delete = FeedsDataGateway.deleteFeed user.Id

    member this.ListApi _ =
        FeedsApi.list this.List

    member this.CreateApi =
        this.Create
        |> FeedsApi.create
        |> bindJson

    member this.UpdateApi feedId =
        feedId
        |> this.Update
        |> FeedsApi.update
        |> bindJson

    member this.DeleteApi feedId =
        FeedsApi.delete (fun _ -> this.Delete feedId)


type private UserArticles(user: User) =
    member this.ListRecent =
        UserArticlesService.listRecent user

    member this.ListRecentApi(_, ctx: HttpContext) =
        let maybeFeedId =
            ctx.TryGetQueryStringValue "feedId" |> Option.bind paramToId

        maybeFeedId
        |> this.ListRecent
        |> ArticlesApi.list

    member this.ListBookmarksApi _ =
        ArticlesApi.list (UserArticlesService.listBookmarks user)

    member this.CreateReadArticle articleId =
        { UserId = user.Id
          ArticleId = articleId }
        |> UserArticlesDataGateway.createReadArticle
        |> ArticlesApi.update

    member this.DeleteReadArticle articleId =
        { UserId = user.Id
          ArticleId = articleId }
        |> UserArticlesDataGateway.deleteReadArticle
        |> ArticlesApi.update

    member this.CreateBookmark articleId =
        { UserId = user.Id
          ArticleId = articleId }
        |> UserArticlesDataGateway.createBookmark
        |> ArticlesApi.update

    member this.DeleteBookmark articleId =
        { UserId = user.Id
          ArticleId = articleId }
        |> UserArticlesDataGateway.deleteBookmark
        |> ArticlesApi.update


type private UserReadPage(articles: UserArticles, user: User) =
    member this.Read innerPage _ =
        ReadPage.page articles.ListRecent user innerPage
    member this.ReadFeed feedId =
        ReadPage.page articles.ListRecent user (ReadPage.Recent(Some feedId))


type private UserManagePage(feeds: UserFeeds, user: User) =
    member this.List _ =
        ManagePage.page maxFeeds feeds.List user ManagePage.List
    member this.SearchNone _ =
        ManagePage.page maxFeeds feeds.List user (ManagePage.Search None)
    member this.SearchSome text =
        ManagePage.page maxFeeds feeds.List user (ManagePage.Search (Some text))


let private authenticatedHandler (user: User) =
    let userFeeds = UserFeeds user
    let userArticles = UserArticles user
    let userReadPage = UserReadPage(userArticles, user)
    let userManagePage = UserManagePage(userFeeds, user)

    choose
        [ GET >=> route "/read" >=> redirectTo false "/read/recent"
          GET >=> route "/read/recent" >=> warbler (userReadPage.Read(ReadPage.Recent None))
          GET >=> routef "/read/recent/feed/%d" userReadPage.ReadFeed
          GET >=> route "/read/bookmarks" >=> warbler (userReadPage.Read ReadPage.Bookmarks)

          GET >=> route "/manage" >=> redirectTo false "/manage/list"
          GET >=> route "/manage/list" >=> warbler userManagePage.List
          GET >=> route "/manage/search" >=> warbler userManagePage.SearchNone
          GET >=> routef "/manage/search/%s" userManagePage.SearchSome

          GET >=> route "/api/feeds" >=> warbler userFeeds.ListApi
          POST >=> route "/api/feeds" >=> userFeeds.CreateApi
          PUT >=> routef "/api/feeds/%d" userFeeds.UpdateApi
          DELETE >=> routef "/api/feeds/%d" userFeeds.DeleteApi

          GET >=> route "/api/articles/recent" >=> warbler userArticles.ListRecentApi
          GET >=> route "/api/articles/bookmarks" >=> warbler userArticles.ListBookmarksApi
          POST >=> routef "/api/articles/%d/read" userArticles.CreateReadArticle
          DELETE >=> routef "/api/articles/%d/read" userArticles.DeleteReadArticle
          POST >=> routef "/api/articles/%d/bookmark" userArticles.CreateBookmark
          DELETE >=> routef "/api/articles/%d/bookmark" userArticles.DeleteBookmark

          setStatusCode 404 >=> text "Not Found" ]

let handler: HttpHandler =
    choose
        [ GET >=> route "/" >=> htmlView HomePage.view
          GET >=> route "/login" >=> htmlView (Auth.Web.loginPage false)
          POST >=> route "/login" >=> (Auth.Web.doLogin UsersDataGateway.findByEmail)
          GET >=> route "/logout" >=> Auth.Web.doLogout
          GET >=> route "/register" >=> Auth.Web.registrationPage
          POST >=> route "/api/users" >=> bindJson (UsersApi.create UsersService.create)

          Auth.Web.authenticate authenticatedHandler

          setStatusCode 403 >=> text "Unauthorized" ]

let errorHandler (ex: Exception) (logger: ILogger): HttpHandler =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message
