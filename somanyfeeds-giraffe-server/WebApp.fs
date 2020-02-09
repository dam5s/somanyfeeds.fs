[<RequireQualifiedAccess>]
module SoManyFeedsServer.WebApp

open Giraffe
open Microsoft.Extensions.Logging
open System

let handler: HttpHandler =
    choose
        [ GET >=> route "/" >=> htmlView HomePage.view
          //            GET >=> routef "/hello/%s" indexHandler
          setStatusCode 404 >=> text "Not Found" ]

let errorHandler (ex: Exception) (logger: ILogger): HttpHandler =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message
