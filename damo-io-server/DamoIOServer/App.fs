module DamoIOServer.App

open DamoIOServer.ArticlesDataGateway
open DamoIOServer.SslHandler
open FSharp.Control
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors


let private updatesSequence =
    asyncSeq {
        let tenMinutes = 10 * 1000 * 60

        while true do
            yield FeedsProcessor.processFeeds (Sources.Repository.findAll())
            do! Async.Sleep tenMinutes
    }


let backgroundProcessing =
    AsyncSeq.iter
        Repository.updateAll
        updatesSequence


let handler =
    choose [
        enforceSsl

        path "/" >=> GET >=> ArticlesHandler.list Repository.findAll

        GET >=> Files.browseHome
        NOT_FOUND "not found"
    ]
