module SoManyFeedsServer.WebApp

open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.Redirection
open SoManyFeedsServer
open SoManyFeedsServer.Json


let private authenticatedPage (user : Authentication.User) : WebPart =
    let listFeeds = FeedsPersistence.listFeeds DataAccess.dataSource
    let createFeed = FeedsPersistence.createFeed DataAccess.dataSource
    let updateFeed = FeedsPersistence.updateFeed DataAccess.dataSource
    let deleteFeed = FeedsPersistence.deleteFeed DataAccess.dataSource

    let readPage _ =
        ReadPage.page user

    let managePage _ =
        ManagePage.page listFeeds user

    let listFeeds _ =
        FeedsApi.list (fun _ -> listFeeds user.Id)

    let createFeed =
        deserializeBody
            FeedsApi.Decoders.feedFields
            (FeedsApi.create <| createFeed user.Id)

    let updateFeed feedId =
        deserializeBody
            FeedsApi.Decoders.feedFields
            (FeedsApi.update <| updateFeed user.Id feedId)

    let deleteFeed feedId =
        FeedsApi.delete
            (fun _ -> deleteFeed user.Id feedId)


    choose [
        GET >=> path "/" >=> redirect "/read"
        GET >=> path "/read" >=> request readPage
        GET >=> path "/manage" >=> request managePage

        GET >=> path "/api/feeds" >=> request listFeeds
        POST >=> path "/api/feeds" >=> createFeed
        PUT >=> pathScan "/api/feeds/%d" updateFeed
        DELETE >=> pathScan "/api/feeds/%d" deleteFeed

        GET >=> Files.browseHome
        NOT_FOUND "not found"
    ]


let webPart =
    choose [
        request <| Authentication.authenticate authenticatedPage
        UNAUTHORIZED "unauthorized"
    ]
