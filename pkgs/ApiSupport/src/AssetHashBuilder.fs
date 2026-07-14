module ApiSupport.AssetHashBuilder

open Microsoft.AspNetCore.Http
open System
open System.Collections.Concurrent
open System.Security.Cryptography
open WebOptimizer

type AssetHashBuilder(ctxAccessor: IHttpContextAccessor, assetPipeline: IAssetPipeline) =

    let hashesByRoute = ConcurrentDictionary<string, string>()

    let tryGetCachedHash (route: string) =
        match hashesByRoute.TryGetValue route with
        | true, value -> Some value
        | false, _ -> None

    let addHashToCache (route: string, hash: string) : string =
        hashesByRoute.[route] <- hash
        hash

    let computeAssetVersionHash (asset: IAsset) =
        task {
            let! bytes = asset.ExecuteAsync(ctxAccessor.HttpContext, WebOptimizerOptions())

            return Convert.ToBase64String(MD5.HashData(bytes)).Replace("=", "")
        }

    let fallbackAssetVersionHash () = task { return "no-version" }

    let computeAndCacheRouteHash (assetRoute: string) =
        task {
            let found, asset = assetPipeline.TryGetAssetFromRoute(assetRoute)

            let! newHash =
                if found then
                    computeAssetVersionHash asset
                else
                    fallbackAssetVersionHash ()

            return addHashToCache (assetRoute, newHash)
        }

    let routeVersionHash assetRoute =
        task {
            match tryGetCachedHash assetRoute with
            | Some hash -> return hash
            | None -> return! computeAndCacheRouteHash assetRoute
        }

    member _.GetPathAsync(assetRoute: string) =
        task {
            let! computedHash = routeVersionHash assetRoute
            return $"%s{assetRoute}?v=%s{computedHash}"
        }
