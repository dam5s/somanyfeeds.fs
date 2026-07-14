module Damo.Io.Server.App

open Giraffe
open Microsoft.Extensions.Logging
open System

open Damo.Io.Server.ArticlesHandler
open Damo.Io.Server.ErrorPageHandler
open ApiSupport.IHttpHandler

[<RequireQualifiedAccess>]
module App =
    let handler: HttpHandler =
        choose
            [ GET >=> route "/" >=> handler<ListArticlesHandler>
              ErrorPageHandler.notFound ]

    let errorHandler (ex: Exception) (logger: ILogger) : HttpHandler =
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        ErrorPageHandler.serverError
