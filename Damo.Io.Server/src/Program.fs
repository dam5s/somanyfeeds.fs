module Program

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open System.IO
open WebOptimizer

open DamoIoServer.App
open DamoIoServer.AppConfig
open DamoIoServer.ArticlesRepository
open DamoIoServer.BackgroundProcessor
open DamoIoServer.AssetHashBuilder

let private configureErrorHandling (app: IApplicationBuilder) =
    if AppConfig.enableExceptionPage then
        app.UseDeveloperExceptionPage()
    else
        app.UseGiraffeErrorHandler App.errorHandler

let private configureApp (app: IApplicationBuilder) =
    (configureErrorHandling app)
        .UseHttpsRedirection()
        .UseWebOptimizer()
        .UseStaticFiles()
        .UseGiraffe(App.handler)

let private configureAssetPipeline (pipeline: IAssetPipeline) =
    pipeline.AddCssBundle("/styles/app.min.css", [| "/styles/reset.css"; "/styles/app.css" |])
    |> ignore

let private configureServices (services: IServiceCollection) =
    services
        .AddGiraffe()
        .AddWebOptimizer(configureAssetPipeline)
        .AddSingleton<AssetHashBuilder>()
    |> ignore

let private configureLogging (builder: ILoggingBuilder) =
    builder.ClearProviders().AddConsole() |> ignore

let webHostBuilder () =
    let webRoot = Path.Combine(AppConfig.contentRoot, "www")

    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(AppConfig.contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .UseUrls($"http://0.0.0.0:%s{AppConfig.serverPort}")
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)

[<EntryPoint>]
let main _ =
    let webHost = webHostBuilder().Build()

    let loggerProvider = webHost.Services.GetRequiredService<ILoggerProvider>()
    let logger = loggerProvider.CreateLogger("Damo.Io.Server.FeedsProcessor")
    let processor = BackgroundProcessor(logger, ArticlesRepository.updateAll)

    processor.StartProcessing()

    webHost.Run()
    0
