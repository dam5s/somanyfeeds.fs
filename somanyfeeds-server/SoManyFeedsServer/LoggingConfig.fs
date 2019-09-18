module SoManyFeedsServer.LoggingConfig

open Logary
open Logary.Configuration
open Logary.Targets


let configure _ =
  let threadId =
      fun next msg ->
          Message.setContext "managedThreadId" (System.Threading.Thread.CurrentThread.ManagedThreadId) msg
          |> next

  Config.create "SoManyFeeds" "localhost"
  |> Config.ilogger (ILogger.LiterateConsole Info)
  |> Config.middleware threadId
  |> Config.target (LiterateConsole.create LiterateConsole.empty "console")
  |> Config.processing (Events.events |> Events.sink [ "console" ])
  |> Config.build
  |> Hopac.Hopac.run
  |> ignore
