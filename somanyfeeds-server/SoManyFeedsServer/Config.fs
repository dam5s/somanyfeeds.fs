module SoManyFeedsServer.Config

open Suave
open DotLiquid
open System.IO


let private contentRoot : string =
    Env.varDefault "CONTENT_ROOT" Directory.GetCurrentDirectory

let private templatesFolder : string =
    Path.Combine (contentRoot, "Resources/templates")

let private publicFolder : string =
    Path.Combine (contentRoot, "Resources/public")

let private defaultPort _ : string =
    "8080"


let port : int =
    Env.varDefaultParse int "PORT" defaultPort


let create : SuaveConfig =
    setTemplatesDir templatesFolder
    setCSharpNamingConvention ()

    let binding = Http.HttpBinding.createSimple HTTP "0.0.0.0" port

    { defaultConfig with
        homeFolder = Some publicFolder
        bindings = [ binding ]
    }
