module Program

open FeedsProcessing.DataGateway
open FeedsProcessing.XmlFeedDecoder
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System.IO

open ApiSupport.AssetHashBuilder
open ApiSupport.AssetPipelineExtensions
open Damo.Io.Server.App
open Damo.Io.Server.AppConfig
open Damo.Io.Server.ArticlesHandler
open Damo.Io.Server.BackgroundProcessor
open Damo.Io.Server.FeedsProcessor
open Damo.Io.Server.LayoutTemplate
open FeedsPersistence.ArticlesRepository
open FeedsPersistence.FeedsRepository

let private configureErrorHandling (app: WebApplication) =
    if AppConfig.enableExceptionPage then
        app.UseDeveloperExceptionPage()
    else
        app.UseGiraffeErrorHandler App.errorHandler

let private configureApp (app: WebApplication) : WebApplication =
    (configureErrorHandling app) //
        .UseHttpsRedirection()
        .UseWebOptimizer()
        .UseStaticFiles()
        .UseGiraffe(App.handler)
    |> always app

let private configureAssetPipeline (pipeline: AssetPipeline) =
    pipeline.AddCssBundle("/styles/app.min.css", [| "/styles/reset.css"; "/styles/app.css" |])

let private configureServices (builder: WebApplicationBuilder) =
    builder.Services
        .AddGiraffe()
        .ConfigurePipeline(configureAssetPipeline)
        .AddHttpContextAccessor()
        .AddSingleton<ArticlesRepository>()
        .AddSingleton<FeedsRepository>()

        .AddSingleton<AssetHashBuilder>()
        .AddSingleton<LayoutTemplate>()
        .AddSingleton<ListArticlesHandler>()

        .AddSingleton<DataGateway>()
        .AddSingleton<XmlFeedDecoder>()
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
