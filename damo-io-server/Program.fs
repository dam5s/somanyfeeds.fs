module Program

open System
open System.IO
open Suave
open Suave.DotLiquid
open DamoIOServer


let private portFromEnv: int =
    try
        "PORT"
        |> Environment.GetEnvironmentVariable
        |> int
    with
    | _ ->
        8080


[<EntryPoint>]
let main _ =
    Async.Start App.backgroundProcessing

    let contentRoot = Directory.GetCurrentDirectory ()
    let templatesFolder = Path.Combine (contentRoot, "Resources/templates")
    let publicFolder = Path.Combine (contentRoot, "Resources/public")

    setTemplatesDir templatesFolder
    setCSharpNamingConvention ()

    let binding = Http.HttpBinding.createSimple HTTP "0.0.0.0" portFromEnv
    let config =
        { defaultConfig with
            homeFolder = Some publicFolder
            bindings = [ binding ]
        }

    startWebServer config App.handler
    0
