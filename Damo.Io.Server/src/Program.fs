module Program

open DamoIoServer.App
open DamoIoServer.FeedsProcessor
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System.IO
open WebOptimizer

open DamoIoServer.AppConfig
open DamoIoServer.BackgroundProcessor
open DamoIoServer.AssetHashBuilder

let private configureErrorHandling (app: WebApplication) =
    if AppConfig.enableExceptionPage then
        app.UseDeveloperExceptionPage()
    else
        app.UseGiraffeErrorHandler App.errorHandler

let private configureApp (app: WebApplication) : WebApplication =
    (configureErrorHandling app)
        .UseHttpsRedirection()
        .UseWebOptimizer()
        .UseStaticFiles()
        .UseGiraffe(App.handler)
    |> always app

let private configureAssetPipeline (pipeline: IAssetPipeline) =
    pipeline.AddCssBundle("/styles/app.min.css", [| "/styles/reset.css"; "/styles/app.css" |])
    |> ignore

let private configureServices (builder: WebApplicationBuilder) =
    builder.Services
        .AddGiraffe()
        .AddWebOptimizer(configureAssetPipeline)
        .AddSingleton<AssetHashBuilder>()
        .AddSingleton<FeedsProcessor>()
        .AddSingleton<IHostedService, FeedsProcessorHostedService>()
    |> always builder

let private configureLogging (builder: ILoggingBuilder) =
    builder.ClearProviders().AddConsole() |> ignore

let private configureWebHost (builder: WebApplicationBuilder) =
    builder.WebHost
        .UseKestrel()
        .UseIISIntegration()
        .UseUrls($"http://0.0.0.0:%s{AppConfig.serverPort}")
        .ConfigureLogging(configureLogging)
    |> always builder

[<EntryPoint>]
let main args =
    let webRoot = Path.Combine(AppConfig.contentRoot, "www")

    let options =
        WebApplicationOptions(Args = args, ContentRootPath = AppConfig.contentRoot, WebRootPath = webRoot)

    let builder =
        WebApplication.CreateBuilder(options) |> configureWebHost |> configureServices

    let app = builder.Build() |> configureApp

    app.Run()
    0
