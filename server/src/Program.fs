module Server.Program

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe


module Views =
    open GiraffeViewEngine

    let layout (content : XmlNode list) =
        html [] [
            head [] [
                meta [ _charset "utf-8" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]


module App =

    let private processFeeds =
        FeedsProcessor.processFeeds
            Articles.Data.Repository.updateAll
            Feeds.Repository.findAll


    let backgroundProcessing =
        let tenMinutes = 10 * 60 * 1000

        async {
            while true do
                processFeeds
                do! Async.Sleep tenMinutes
        }


    let private articlesListHandler =
        Articles.Handlers.list Views.layout Articles.Data.Repository.findAll


    let handler =
        choose [
            GET >=>
                choose [
                    route "/" >=> articlesListHandler
                ]
            setStatusCode 404 >=> text "Not Found"
        ]


let errorHandler (ex : Exception) (logger : ILogger) : HttpHandler =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message


let configureApp (app : IApplicationBuilder) =
    (app.UseGiraffeErrorHandler errorHandler)
        .UseStaticFiles()
        .UseGiraffe(App.handler)


let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore


let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Warning

    builder
        .AddFilter(filter)
        .AddConsole()
        .AddDebug() |> ignore


[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")

    Async.Start App.backgroundProcessing

    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()

    0
