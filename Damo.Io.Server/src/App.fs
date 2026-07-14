module Damo.Io.Server.App

open Giraffe
open Microsoft.Extensions.Logging
open System

open Damo.Io.Server.IHttpHandler
open Damo.Io.Server.ArticlesHandler

[<RequireQualifiedAccess>]
module App =
    let handler: HttpHandler =
        choose
            [ GET >=> route "/" >=> handler<ListArticlesHandler>
              setStatusCode 404 >=> text "Not Found" ]

    let errorHandler (ex: Exception) (logger: ILogger) : HttpHandler =
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message
