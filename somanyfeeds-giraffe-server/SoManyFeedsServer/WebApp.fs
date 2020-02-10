[<RequireQualifiedAccess>]
module SoManyFeedsServer.WebApp

open Giraffe
open Microsoft.Extensions.Logging
open SoManyFeeds
open SoManyFeedsServer
open System

let handler: HttpHandler =
    choose
        [ GET >=> route "/" >=> htmlView HomePage.view
          GET >=> route "/login" >=> htmlView (Auth.Web.loginPage false)
          POST >=> route "/login" >=> (Auth.Web.doLogin UsersDataGateway.findByEmail)
          GET >=> route "/logout" >=> Auth.Web.doLogout

          Auth.Web.authenticate (fun user -> choose [
              GET >=> route "/read" >=> text (sprintf "Read %s" user.Name)
              GET >=> route "/manage" >=> text (sprintf "Manage %s" user.Name)
          ])

          setStatusCode 404 >=> text "Not Found" ]

let errorHandler (ex: Exception) (logger: ILogger): HttpHandler =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message
