module DamoIoServer.AssetHashBuilder

open System
open System.Security.Cryptography

open System.Collections.Concurrent
open Microsoft.AspNetCore.Http
open WebOptimizer

type AssetHashBuilder(assetPipeline: IAssetPipeline) =

    let hashesByRoute = ConcurrentDictionary<string, string>()

    let tryGetCachedHash (route: string) =
        match hashesByRoute.TryGetValue route with
        | true, value -> Some value
        | false, _ -> None

    let addHashToCache (route: string, hash: string) : string =
        hashesByRoute.[route] <- hash
        hash

    let computeAssetVersionHash (ctx: HttpContext) (asset: IAsset) =
        let bytes =
            asset.ExecuteAsync(ctx, WebOptimizerOptions())
            |> Async.AwaitTask
            |> Async.RunSynchronously

        Convert.ToBase64String(MD5.HashData(bytes)).Replace("=", "")

    let computeAndCacheRouteHash (ctx: HttpContext) (assetRoute: string) =
        let found, asset = assetPipeline.TryGetAssetFromRoute(assetRoute)

        let newHash =
            if found then
                computeAssetVersionHash ctx asset
            else
                "no-version"

        addHashToCache (assetRoute, newHash)

    let routeVersionHash (ctx: HttpContext) assetRoute =
        match tryGetCachedHash assetRoute with
        | Some hash -> hash
        | None -> computeAndCacheRouteHash ctx assetRoute

    member this.Path (ctx: HttpContext) (assetRoute: string) =
        $"%s{assetRoute}?v=%s{routeVersionHash ctx assetRoute}"
