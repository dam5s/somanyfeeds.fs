module Program

open DamoIOServer
open Giraffe
open Giraffe.Serialization.Json
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.FSharpLu.Json
open Newtonsoft.Json
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
    services .AddGiraffe() |> ignore

    let customSettings = JsonSerializerSettings()
    customSettings.Converters.Add(CompactUnionJsonConverter(true))

    services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer(customSettings)) |> ignore

let private configureLogging (builder: ILoggingBuilder) =
    builder
        .AddConsole()
        .AddDebug()
        |> ignore

let webHostBuilder logary =
    let contentRoot = Env.varDefault "CONTENT_ROOT" Directory.GetCurrentDirectory
    let webRoot = Path.Combine(contentRoot, "WebRoot")
    let builder = IWebHostBuilderEx.addLogary(WebHostBuilder(), logary)

    builder
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)

[<EntryPoint>]
let main _ =
    let logary = LoggingConfig.configure()

    Async.Start App.backgroundProcessing
    webHostBuilder(logary).Build().Run()
    0
