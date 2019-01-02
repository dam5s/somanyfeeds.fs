module SoManyFeedsServer.WebApp

open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.Redirection
open SoManyFeedsServer
open SoManyFeedsServer.Json


let private authenticatedPage (user : Authentication.User) : WebPart =

    let listFeeds _ = FeedsPersistence.listFeeds DataAccess.dataSource user.Id
    let createFeed = FeedsPersistence.createFeed DataAccess.dataSource user.Id
    let updateFeed = FeedsPersistence.updateFeed DataAccess.dataSource user.Id
    let deleteFeed = FeedsPersistence.deleteFeed DataAccess.dataSource user.Id
    let listRecentArticles _ = UserArticlesService.listRecent DataAccess.dataSource user


    let readPage _ =
        ReadPage.page user

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
        GET >=> path "/" >=> redirect "/read"
        GET >=> path "/read" >=> request readPage
        GET >=> path "/manage" >=> request managePage

        GET >=> path "/api/feeds" >=> request listFeedsApi
        POST >=> path "/api/feeds" >=> createFeedApi
        PUT >=> pathScan "/api/feeds/%d" updateFeedApi
        DELETE >=> pathScan "/api/feeds/%d" deleteFeedApi

        GET >=> path "/api/articles/recent" >=> request listRecentArticlesApi

        GET >=> Files.browseHome
        NOT_FOUND "not found"
    ]


let webPart =
    choose [
        request <| Authentication.authenticate authenticatedPage
        UNAUTHORIZED "unauthorized"
    ]
