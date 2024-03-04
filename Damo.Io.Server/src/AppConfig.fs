module DamoIoServer.AppConfig

open System.IO

[<RequireQualifiedAccess>]
module AppConfig =
    let serverPort = Env.varDefault "PORT" (always "9000")

    let contentRoot = Env.varDefault "CONTENT_ROOT" Directory.GetCurrentDirectory

    let assetMinificationDisabled =
        Env.var "DISABLE_MINIFIED_ASSETS" |> Option.hasValue "true"

    let assetMinificationEnabled = not assetMinificationDisabled

    let enableExceptionPage = Env.var "ENABLE_EXCEPTION_PAGE" |> Option.hasValue "true"
