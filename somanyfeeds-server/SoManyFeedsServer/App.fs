module SoManyFeedsServer.App

open Npgsql
open System.Data.Common
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.Redirection
open SoManyFeedsServer
open SoManyFeedsServer.Json
open SoManyFeedsServer.DataSource


module private DataAccess =
    let private connectionString = "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev"

    let private dataSource : DataSource =
        fun _ ->
            try
                Ok (new NpgsqlConnection (connectionString) :> DbConnection)
            with
            | ex ->
                Error <| sprintf "Connection error: %s" ex.Message


    let listFeeds = FeedsPersistence.listFeeds dataSource
    let createFeed = FeedsPersistence.createFeed dataSource
    let updateFeed = FeedsPersistence.updateFeed dataSource
    let deleteFeed = FeedsPersistence.deleteFeed dataSource


let private authenticatedPage (user : Authentication.User) : WebPart =
    let readPage _ =
        ReadPage.page user

    let managePage _ =
        ManagePage.page DataAccess.listFeeds user

    let listFeeds _ =
        FeedsApi.list (fun _ -> DataAccess.listFeeds user.Id)

    let createFeed =
        deserializeBody
            FeedsApi.Decoders.feedFields
            (FeedsApi.create <| DataAccess.createFeed user.Id)

    let updateFeed feedId =
        deserializeBody
            FeedsApi.Decoders.feedFields
            (FeedsApi.update <| DataAccess.updateFeed user.Id feedId)

    let deleteFeed feedId =
        FeedsApi.delete
            (fun _ -> DataAccess.deleteFeed user.Id feedId)


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
