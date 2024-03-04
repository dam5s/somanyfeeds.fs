module DamoIoServer.BackgroundProcessor

open FSharp.Control
open Microsoft.Extensions.Logging

open DamoIoServer.SourcesRepository
open DamoIoServer.ArticlesRepository
open DamoIoServer.FeedsProcessor

[<RequireQualifiedAccess>]
type BackgroundProcessor(logger: ILogger, updateArticles: ArticlesRepository.UpdateArticles) =

    let updatesSequence () =
        asyncSeq {
            let tenMinutes = 10 * 1000 * 60

            while true do
                let! newArticles =
                    SourcesRepository.findAll ()
                    |> FeedsProcessor.processFeeds logger
                    |> AsyncSeq.toListAsync

                yield newArticles

                do! Async.Sleep tenMinutes
        }

    member this.StartProcessing() =
        Async.Start(updatesSequence () |> AsyncSeq.iter updateArticles)
