module DamoIoServer.BackgroundProcessor

open System
open System.Threading
open System.Threading.Tasks
open FSharp.Control
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open DamoIoServer.SourcesRepository
open DamoIoServer.ArticlesRepository
open DamoIoServer.FeedsProcessor

type FeedsProcessorHostedService(logger: ILogger<FeedsProcessorHostedService>, processor: FeedsProcessor) =

    let stopRequested = new CancellationTokenSource()
    let stopCompleted = TaskCompletionSource()
    let mutable runningUpdatesTask: Task<unit> option = None

    let updatesTask (cancellationToken: CancellationToken) () : Task<unit> =
        task {
            let tenMinutes = TimeSpan.FromMinutes(10.0)

            try
                cancellationToken.ThrowIfCancellationRequested()

                while not stopRequested.Token.IsCancellationRequested do
                    logger.LogInformation("Periodic source updates starting")

                    let sources = SourcesRepository.findAll ()
                    let! newArticles = processor.ProcessFeeds(sources, stopRequested.Token) |> TaskSeq.toListAsync
                    ArticlesRepository.updateAll newArticles

                    logger.LogInformation("Periodic source updates done")

                    do! Task.Delay(tenMinutes, stopRequested.Token)

            with :? OperationCanceledException ->
                stopCompleted.TrySetResult() |> ignore
                logger.LogInformation("Updates task cancelled — shutting down")
        }

    interface IHostedService with
        member _.StartAsync(cancellationToken) =
            task { runningUpdatesTask <- Some(Task.Run<unit>(updatesTask cancellationToken)) }

        member _.StopAsync(cancellationToken) =
            task {
                stopRequested.Cancel()
                do! stopCompleted.Task.WaitAsync(cancellationToken)
            }
