module SoManyFeedsServer.LoggingConfig

open Logary
open Logary.Configuration
open Logary.Targets


let configure _ =
  Config.create "SoManyFeeds" "localhost"
  |> Config.target (LiterateConsole.create LiterateConsole.empty "console")
  |> Config.ilogger (ILogger.LiterateConsole Info)
  |> Config.build
  |> Hopac.Hopac.run
