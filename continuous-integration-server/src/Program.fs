module CIServer.Program


open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.AsyncSeqExtensions
open FSharp.Control
open Giraffe
open Build


module Views =
    open GiraffeViewEngine

    let layout (content : XmlNode list) =
        html [] [
            head [] [
                meta [ _charset "utf-8" ]
                meta [ _name "viewport" ; _content "width=device-width" ]
                link [ _rel  "stylesheet" ; _type "text/css" ; _href "/app.css" ]
                title [] [ rawText "damo.io - Continuous integration" ]
            ]
            body [] content
        ]


module App =
    open Git

    let private gitRepo = GitRepository "https://github.com/dam5s/somanyfeeds.fs.git"

    let private dockerScript : DockerScript = { image = "dam5s/somanyfeeds-ci" ; scriptName = "build" }


    let private newSha (maybeBuild : Build option) (sha : Sha) : Result<Sha, string> =
        match maybeBuild with
        | None -> Ok sha
        | Some build ->
            if build.sha = sha then
                Error "Sha not changed"
            else
                Ok sha


    let private buildSequence : AsyncSeq<Result<Build, string>> =
        let oneMinute = 1 * 1000 * 60

        asyncSeq {
            while true do
                let latestBuild = BuildRepository.latest ()

                let newBuild = Git.latestSha gitRepo
                               |> Result.bind (newSha latestBuild)
                               |> Result.map BuildRepository.create

                yield newBuild

                do! Async.Sleep oneMinute
        }


    let integrationWorker =
        let runBuild = Result.map (BuildRunner.run dockerScript)

        buildSequence |> AsyncSeq.iter (ignore << runBuild)


    let buildListHandler =
        BuildHandlers.list Views.layout BuildRepository.findLast10


    let handler =
        choose [
            GET >=>
                choose [
                    route "/" >=> buildListHandler
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
    services
        .AddGiraffe()
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

    Async.Start App.integrationWorker

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
