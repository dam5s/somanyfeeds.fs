module SoManyFeedsServer.Config

open Suave
open DotLiquid
open System.IO
open System


let private contentRoot =
    match Environment.GetEnvironmentVariable "CONTENT_ROOT" with
    | null
    | "" -> Directory.GetCurrentDirectory ()
    | path -> path

let private templatesFolder = Path.Combine (contentRoot, "Resources/templates")
let private publicFolder = Path.Combine (contentRoot, "Resources/public")


let port: int =
    try
        int <| Environment.GetEnvironmentVariable "PORT"
    with
    | _ ->
        8080


let create : SuaveConfig =
    setTemplatesDir templatesFolder
    setCSharpNamingConvention ()

    let binding = Http.HttpBinding.createSimple HTTP "0.0.0.0" port

    { defaultConfig with
        homeFolder = Some publicFolder
        bindings = [ binding ]
    }
