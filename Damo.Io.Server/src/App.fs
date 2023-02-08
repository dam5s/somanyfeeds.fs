module DamoIoServer.App

open System
open DamoIoServer
open FSharp.Control
open Giraffe
open Microsoft.Extensions.Logging


let private updatesSequence logger =
    asyncSeq {
        let tenMinutes = 10 * 1000 * 60

        while true do
            let! newArticles =
                SourcesRepository.findAll ()
                |> FeedsProcessor.processFeeds logger
                |> AsyncSeq.toListAsync

            yield newArticles

            do! Async.Sleep tenMinutes
    }

let backgroundProcessing logger =
    AsyncSeq.iter
        ArticlesRepository.updateAll
        (updatesSequence logger)

let handler: HttpHandler =
    choose
        [ GET >=> route "/" >=> redirectTo false "/About,Social,Blog"
          GET >=> routef "/%s" (ArticlesHandler.list ArticlesRepository.findAllBySources)
          setStatusCode 404 >=> text "Not Found" ]

let errorHandler (ex: Exception) (logger: ILogger): HttpHandler =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message
