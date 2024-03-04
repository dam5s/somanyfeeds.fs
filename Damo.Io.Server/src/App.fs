module DamoIoServer.App

open System
open Giraffe
open Microsoft.Extensions.Logging

open DamoIoServer.ArticlesRepository

[<RequireQualifiedAccess>]
module App =
    let handler: HttpHandler =
        choose
            [ GET >=> route "/" >=> redirectTo false "/About,Social,Blog"
              GET >=> routef "/%s" (ArticlesHandler.list ArticlesRepository.findAllBySources)
              setStatusCode 404 >=> text "Not Found" ]

    let errorHandler (ex: Exception) (logger: ILogger) : HttpHandler =
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message
