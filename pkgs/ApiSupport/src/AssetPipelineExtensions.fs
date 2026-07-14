module ApiSupport.AssetPipelineExtensions

open System.Runtime.CompilerServices
open Microsoft.Extensions.DependencyInjection
open WebOptimizer

type AssetPipeline(webOptimizerPipeline: IAssetPipeline) =

    member _.AddCssBundle(route: string, sourceFiles: string array) =
        webOptimizerPipeline.AddCssBundle(route, sourceFiles)

type AssetPipelineExtensions() =

    [<Extension>]
    static member inline ConfigurePipeline(services: IServiceCollection, configuration: AssetPipeline -> 'a) =
        services.AddWebOptimizer(fun p -> configuration (AssetPipeline(p)) |> ignore)
