module SoManyFeedsServer.WebConfig

open Suave
open System.IO


type private JsonCookieSerialiser() =
    interface CookieSerialiser with

        member x.serialise (map: Map<string, obj>): byte [] =
            map
            |> Json.serializeSimpleMap
            |> UTF8.bytes

        member x.deserialise bytes =
            bytes
            |> UTF8.toString
            |> Json.deserializeSimpleMap


let private contentRoot =
    Env.varDefault "CONTENT_ROOT" Directory.GetCurrentDirectory

let private templatesFolder =
    Path.Combine(contentRoot, "Resources", "templates")

let private publicFolder =
    Path.Combine(contentRoot, "Resources", "public")


let port =
    Env.varDefaultParse int "PORT" (always "8080")


let create =
    DotLiquid.setTemplatesDir templatesFolder
    DotLiquid.setCSharpNamingConvention()

    let binding = Http.HttpBinding.createSimple HTTP "0.0.0.0" port

    { defaultConfig with
        homeFolder = Some publicFolder
        bindings = [ binding ]
        serverKey = ServerKey.fromBase64 (Env.varRequired "COOKIE_ENCRYPTION_KEY")
        cookieSerialiser = JsonCookieSerialiser()
    }
