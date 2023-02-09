module Program

open DamoIoServer
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open System.IO

let private configureErrorHandling (app: IApplicationBuilder) =
    let config = app.ApplicationServices.GetService<IConfiguration>()

    let enableExceptionPage =
        match config.GetValue<string>("ENABLE_EXCEPTION_PAGE") with
        | "true" -> true
        | _ -> false

    match enableExceptionPage with
    | true -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler App.errorHandler

let private configureApp (app: IApplicationBuilder) =
    (configureErrorHandling app)
        .UseHttpsRedirection()
        .UseStaticFiles()
        .UseGiraffe(App.handler)

let private configureServices (services: IServiceCollection) =
    services
        .AddGiraffe()
        |> ignore

let private configureLogging (builder: ILoggingBuilder) =
    builder
        .ClearProviders()
        .AddConsole()
        |> ignore

let webHostBuilder () =
    let serverPort = Env.varDefault "PORT" (always "9000")
    let contentRoot = Env.varDefault "CONTENT_ROOT" Directory.GetCurrentDirectory
    let webRoot = Path.Combine(contentRoot, "www")

    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .UseUrls($"http://0.0.0.0:%s{serverPort}")
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)

[<EntryPoint>]
let main _ =
    let webHost = webHostBuilder().Build()

    let loggerProvider = webHost.Services.GetRequiredService<ILoggerProvider>()
    let processorLogger = loggerProvider.CreateLogger("Damo.Io.Server.FeedsProcessor")
    Async.Start (App.backgroundProcessing processorLogger)

    webHost.Run()
    0
