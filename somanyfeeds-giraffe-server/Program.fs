module Program

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open System.IO
open SoManyFeeds
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
    (configureErrorHandling app)
        .UseHttpsRedirection()
        .UseStaticFiles()
        .UseGiraffe(WebApp.handler)

let private configureServices (services: IServiceCollection) =
    services
        .AddGiraffe()
        |> ignore

let private configureLogging (builder: ILoggingBuilder) =
    builder
        .AddFilter(fun l -> l.Equals LogLevel.Error)
        .AddConsole()
        .AddDebug()
        |> ignore
        
let private runServer _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

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

[<EntryPoint>]
let main args =
    args
    |> Array.tryHead
    |> Option.bind Tasks.run
    |> Option.defaultWith (fun _ ->
        Async.Start FeedsProcessor.backgroundProcessingInfinite
        runServer ()
    )
    0
