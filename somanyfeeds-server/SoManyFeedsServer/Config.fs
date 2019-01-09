module SoManyFeedsServer.Config

open Suave
open DotLiquid
open System.IO


type private JsonCookieSerialiser () =
    interface CookieSerialiser with

        member x.serialise (map: Map<string, obj>) : byte[] =
            map
            |> Json.serializeSimpleMap
            |> UTF8.bytes

        member x.deserialise bytes =
            bytes
            |> UTF8.toString
            |> Json.deserializeSimpleMap


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
        serverKey = ServerKey.fromBase64 (Env.varRequired "COOKIE_ENCRYPTION_KEY")
        cookieSerialiser = new JsonCookieSerialiser()
    }
