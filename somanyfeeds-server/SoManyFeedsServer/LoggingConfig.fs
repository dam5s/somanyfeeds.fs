module SoManyFeedsServer.LoggingConfig

open Logary
open Logary.Adapters.Facade
open Logary.Configuration
open Logary.Targets


let configure _ =
  let threadId =
      fun next msg ->
          Message.setContext "managedThreadId" (System.Threading.Thread.CurrentThread.ManagedThreadId) msg
          |> next

  let logary = Config.create "SoManyFeeds" "localhost"
               |> Config.ilogger (ILogger.LiterateConsole Info)
               |> Config.middleware threadId
               |> Config.target (LiterateConsole.create LiterateConsole.empty "console")
               |> Config.processing (Events.events |> Events.sink ["console"])
               |> Config.build
               |> Hopac.Hopac.run

  LogaryFacadeAdapter.initialise<Suave.Logging.Logger> logary

  ()
