module SoManyFeedsServer.CacheBusting

let assetPath (path: string) =
    sprintf "%s?v=dev" path
