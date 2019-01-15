module SoManyFeedsServer.WebApp

open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open SoManyFeedsServer
open SoManyFeedsServer.Json


let private authenticatedPage (user : Authentication.User) : WebPart =

    let listFeeds _ = FeedsPersistence.listFeeds DataAccess.dataSource user.Id
    let createFeed = FeedsPersistence.createFeed DataAccess.dataSource user.Id
    let updateFeed = FeedsPersistence.updateFeed DataAccess.dataSource user.Id
    let deleteFeed = FeedsPersistence.deleteFeed DataAccess.dataSource user.Id
    let listRecentArticles _ = UserArticlesService.listRecent DataAccess.dataSource user


    let readPage _ =
        ReadPage.page listRecentArticles user

    let managePage _ =
        ManagePage.page listFeeds user

    let listFeedsApi _ =
        FeedsApi.list listFeeds

    let createFeedApi =
        deserializeBody
            FeedsApi.Decoders.feedFields
            (FeedsApi.create createFeed)

    let updateFeedApi feedId =
        deserializeBody
            FeedsApi.Decoders.feedFields
            (FeedsApi.update <| updateFeed feedId)

    let deleteFeedApi feedId =
        FeedsApi.delete
            (fun _ -> deleteFeed feedId)

    let listRecentArticlesApi _ =
        ArticlesApi.list listRecentArticles


    choose [
        GET >=> path "/read" >=> request readPage
        GET >=> path "/manage" >=> request managePage

        GET >=> path "/api/feeds" >=> request listFeedsApi
        POST >=> path "/api/feeds" >=> createFeedApi
        PUT >=> pathScan "/api/feeds/%d" updateFeedApi
        DELETE >=> pathScan "/api/feeds/%d" deleteFeedApi

        GET >=> path "/api/articles/recent" >=> request listRecentArticlesApi

        NOT_FOUND "not found"
    ]


let webPart =
    let findByEmail = UsersPersistence.findByEmail DataAccess.dataSource
    let homePage = DotLiquid.page "home.html.liquid" ()

    choose [
        GET >=> path "/" >=> homePage
        GET >=> Files.browseHome
        GET >=> path "/login" >=> request Authentication.loginPage
        POST >=> path "/login" >=> request (Authentication.doLogin findByEmail)
        GET >=> path "/logout" >=> request Authentication.doLogout

        request (Authentication.authenticate authenticatedPage)

        UNAUTHORIZED "unauthorized"
    ]
