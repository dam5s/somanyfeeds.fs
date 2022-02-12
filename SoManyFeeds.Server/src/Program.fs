module Program

open Giraffe
open Giraffe.Serialization.Json
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.StaticFiles
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.FSharpLu.Json
open Newtonsoft.Json
open System
open System.IO
open SoManyFeedsPersistence
open SoManyFeedsServer

let private configureErrorHandling (app: IApplicationBuilder) =
    let config = app.ApplicationServices.GetService<IConfiguration>()

    let enableExceptionPage =
        match config.GetValue<string>("ENABLE_EXCEPTION_PAGE") with
        | "true" -> true
        | _ -> false

    match enableExceptionPage with
    | true -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler WebApp.errorHandler

let private configureApp (app: IApplicationBuilder) =
    let contentTypeProvider = FileExtensionContentTypeProvider()
    contentTypeProvider.Mappings.[".webmanifest"] <- "application/json"
    let staticFileOptions = StaticFileOptions()
    staticFileOptions.ContentTypeProvider <- contentTypeProvider

    (configureErrorHandling app)
        .UseHttpsRedirection()
        .UseStaticFiles(staticFileOptions)
        .UseGiraffe(WebApp.handler)

let private configureServices (services: IServiceCollection) =
    services .AddGiraffe() |> ignore

    let customSettings = JsonSerializerSettings()
    customSettings.Converters.Add(CompactUnionJsonConverter(true))

    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer(customSettings)) |> ignore

let private configureLogging (builder: ILoggingBuilder) =
    builder
        .AddConsole()
        .AddDebug()
        |> ignore

let webHostBuilder (serverPort, contentRoot, logary) =
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    useLogary(WebHostBuilder(), logary)
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .UseUrls(sprintf "http://0.0.0.0:%s" serverPort)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)

[<EntryPoint>]
let main args =
    let logary = LoggingConfig.configure()
    let contentRoot = Directory.GetCurrentDirectory()
    let serverPort = Env.varDefault "PORT" (always "5000")

    args
    |> Array.tryHead
    |> Option.bind Tasks.run
    |> Option.defaultWith (fun _ ->
        Async.Start FeedsProcessor.backgroundProcessingInfinite
        webHostBuilder(serverPort, contentRoot, logary).Build().Run()
    )
    0
