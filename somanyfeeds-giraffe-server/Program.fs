module SoManyFeedsServer.Program

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.Extensions.Configuration

let private configureApp (app : IApplicationBuilder) =
    let config = app.ApplicationServices.GetService<IConfiguration>()

    let enableExceptionPage =
        match config.GetValue<string>("ENABLE_EXCEPTION_PAGE") with
        | "true" -> true
        | _ -> false

    (match enableExceptionPage with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler WebApp.errorHandler)
        .UseHttpsRedirection()
        .UseStaticFiles()
        .UseGiraffe(WebApp.handler)

let private configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

let private configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore


[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
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
