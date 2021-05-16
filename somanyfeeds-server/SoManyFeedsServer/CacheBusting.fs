module SoManyFeedsServer.CacheBusting

open System.IO

let assetPath (path: string) =
    let version =
        try File.ReadAllText("WebRoot/assets.version")
        with ex -> "dev"

    $"%s{path}?v=%s{version}"
