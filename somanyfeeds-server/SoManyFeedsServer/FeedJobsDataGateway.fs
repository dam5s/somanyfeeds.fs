module SoManyFeedsServer.FeedJobsDataGateway

open System
open FSharp.Data.Sql
open FeedsProcessing.Feeds
open SoManyFeedsServer.DataSource


type JobFailure =
    JobFailure of message:string


let createMissing (dataContext: DataContext) : AsyncResult<int> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            let existingUrls =
                query {
                    for job in ctx.Public.FeedJobs do
                    select job.FeedUrl
                }
            let missingUrls =
                query {
                    for feed in ctx.Public.Feeds do
                    where (feed.Url |<>| existingUrls)
                    select feed.Url
                }
            let updatesCount =
                missingUrls
                |> Seq.map (fun url ->
                    let entity = ctx.Public.FeedJobs.Create ()
                    entity.FeedUrl <- url
                )
                |> Seq.length

            ctx.SubmitUpdates ()
            updatesCount
        }
    }


let startSome (dataContext: DataContext) (howMany : int): AsyncResult<FeedUrl seq> =
    asyncResult {
        let! ctx = dataContext

        let now = DateTime.UtcNow
        let tenMinutesAgo = now.AddMinutes(-10.0)
        let twoMinutesFromNow = now.AddMinutes(2.0)

        return! dataAccessOperation { return fun _ ->
            let updatedUrls =
                query {
                    for job in ctx.Public.FeedJobs do
                    where (
                        (job.LockedUntil.IsNone || job.LockedUntil.Value < now)
                        && (job.CompletedAt.IsNone || job.CompletedAt.Value < tenMinutesAgo)
                    )
                    take howMany
                    select job
                }
                |> Seq.map (fun job ->
                    job.StartedAt <- Some now
                    job.LockedUntil <- Some twoMinutesFromNow

                    ctx.SubmitUpdates ()

                    FeedUrl job.FeedUrl
                )

            updatedUrls
        }
    }


let private updateJob (dataContext: DataContext) (FeedUrl url)  updateFunction : AsyncResult<int> =
    asyncResult {
        let! ctx = dataContext

        return! dataAccessOperation { return fun _ ->
            let updatesCount =
                query {
                    for job in ctx.Public.FeedJobs do
                    where (job.FeedUrl = url)
                    take 1
                }
                |> Seq.map updateFunction
                |> Seq.length

            ctx.SubmitUpdates ()
            updatesCount
        }
    }


let complete (dataContext: DataContext) (url : FeedUrl): AsyncResult<int> =
    let setCompleted (entity : FeedJobEntity) =
        entity.CompletedAt <- Some DateTime.UtcNow
        entity.LockedUntil <- None

    updateJob dataContext url setCompleted


let fail (dataContext: DataContext) (url : FeedUrl) (JobFailure message): AsyncResult<int> =
    let setFailed (entity : FeedJobEntity) =
        entity.LastFailedAt <- Some DateTime.UtcNow
        entity.LastFailure <- message
        entity.LockedUntil <- None

    updateJob dataContext url setFailed
