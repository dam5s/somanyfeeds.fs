module DamoIOServer.App

open FSharp.Control
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open DamoIOServer.Articles.Data
open DamoIOServer.SslHandler


let private updatesSequence : AsyncSeq<Record list> =
    asyncSeq {
        let tenMinutes = 10 * 1000 * 60

        while true do
            yield FeedsProcessor.processFeeds (Feeds.Repository.findAll ())
            do! Async.Sleep tenMinutes
    }


let backgroundProcessing =
    AsyncSeq.iter
        Articles.Data.Repository.updateAll
        updatesSequence


let private articlesList =
    Articles.Handlers.list Articles.Data.Repository.findAll


let handler =
    choose [
        enforceSsl

        path "/" >=> GET >=> articlesList

        GET >=> Files.browseHome
        NOT_FOUND "not found"
    ]
