module SoManyFeedsServer.App

open Npgsql
open System
open System.Data.Common
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.Redirection
open SoManyFeedsServer
open SoManyFeedsServer.DataSource


module private DataAccess =
    open SoManyFeedsServer.Authentication

    let private connectionString = "Host=localhost;Username=somanyfeeds;Password=secret;Database=somanyfeeds_dev"

    let private dataSource : DataSource =
        fun _ ->
            try
                Ok (new NpgsqlConnection (connectionString) :> DbConnection)
            with
            | ex ->
                Error <| String.Format("Connection error: {0}", ex.Message)


    let listFeeds = FeedsPersistence.listFeeds dataSource
    let createFeed = FeedsPersistence.createFeed dataSource
    let updateFeed = FeedsPersistence.updateFeed dataSource
    let deleteFeed = FeedsPersistence.deleteFeed dataSource


let private authenticatedPage (user : Authentication.User) : WebPart =
    choose [
        GET >=> path "/" >=> redirect "/read"
        GET >=> path "/read" >=> ReadPage.page user
        GET >=> path "/manage" >=> ManagePage.page DataAccess.listFeeds user

        GET >=> path "/api/feeds" >=> FeedsApi.list (fun _ -> DataAccess.listFeeds user.Id)
        POST >=> path "/api/feeds" >=> FeedsApi.create (DataAccess.createFeed user.Id)
        PUT >=> pathScan "/api/feeds/%d" (DataAccess.updateFeed user.Id >> FeedsApi.update)
        DELETE >=> pathScan "/api/feeds/%d" ((fun id _ -> DataAccess.deleteFeed user.Id id) >> FeedsApi.delete)

        GET >=> Files.browseHome
        NOT_FOUND "not found"
    ]


let webPart =
    choose [
        Authentication.authenticate authenticatedPage
        UNAUTHORIZED "unauthorized"
    ]
