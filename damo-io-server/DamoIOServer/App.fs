module DamoIOServer.App

open System
open DamoIOServer.ArticlesDataGateway
open FSharp.Control
open Giraffe
open Microsoft.Extensions.Logging


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

let handler: HttpHandler =
    choose
        [ GET >=> route "/" >=> redirectTo false "/About,Social,Blog"
          GET >=> routef "/%s" (ArticlesHandler.list Repository.findAll)
          setStatusCode 404 >=> text "Not Found" ]

let errorHandler (ex: Exception) (logger: ILogger): HttpHandler =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message
