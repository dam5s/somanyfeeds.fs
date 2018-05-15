module Server.Program

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.AsyncSeqExtensions
open Giraffe
open FSharp.Control
open Server.Articles.Data
open Giraffe.Serialization.Json
open Newtonsoft.Json


module Views =
    open GiraffeViewEngine

    let layout (content : XmlNode list) =
        html [] [
            head [] [
                meta [ _charset "utf-8" ]
                link [ _rel  "stylesheet" ; _type "text/css" ; _href "/app.css" ]
                title [] [ rawText "damo.io - Damien Le Berrigaud's feed aggregator." ]
            ]
            body [] content
        ]


module App =
    let private updatesSequence : AsyncSeq<Record list> =
        let tenMinutes = 10 * 1000 * 60

        asyncSeq {
            while true do
                yield FeedsProcessor.processFeeds (Feeds.Repository.findAll ())
                do! Async.Sleep tenMinutes
        }


    let backgroundProcessing =
        updatesSequence
            |> AsyncSeq.iter Articles.Data.Repository.updateAll


    let private articlesListHandler =
        Articles.Handlers.list
            Views.layout
            Articles.Data.Repository.findAll


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
    let jsonSettings = new JsonSerializerSettings()
    jsonSettings.Converters.Add (new Json.OptionConverter())

    services
        .AddGiraffe()
        .AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer(jsonSettings))
        |> ignore


let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Warning

    builder
        .AddFilter(filter)
        .AddConsole()
        .AddDebug()
        |> ignore


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
