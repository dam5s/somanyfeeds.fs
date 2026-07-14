module Damo.Io.Server.BackgroundProcessor

open FSharp.Control
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open System
open System.Threading
open System.Threading.Tasks

open Damo.Io.Server.ArticlesRepository
open Damo.Io.Server.FeedsProcessor
open Damo.Io.Server.FeedsRepository

type FeedsProcessorHostedService
    (
        logger: ILogger<FeedsProcessorHostedService>,
        processor: FeedsProcessor,
        feedsRepo: FeedsRepository,
        articlesRepo: ArticlesRepository
    ) =

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

                    let! feeds = feedsRepo.FindAllAsync()
                    let! newArticles = processor.ProcessFeeds(feeds, stopRequested.Token) |> TaskSeq.toListAsync

                    do! articlesRepo.UpdateAll newArticles

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
