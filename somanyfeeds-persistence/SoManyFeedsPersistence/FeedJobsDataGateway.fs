module SoManyFeedsPersistence.FeedJobsDataGateway

open FSharp.Data.Sql
open FeedsProcessing.Feeds
open SoManyFeedsPersistence.DataSource
open System


type JobFailure =
    JobFailure of message: string


let createMissing =
    dataAccessOperation (fun ctx ->
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
                let entity = ctx.Public.FeedJobs.Create()
                entity.FeedUrl <- url
            )
            |> Seq.length

        ctx.SubmitUpdates()
        updatesCount
    )


let startSome howMany: AsyncResult<FeedUrl seq> =
    dataAccessOperation (fun ctx ->
        let now = DateTime.UtcNow
        let tenMinutesAgo = now.AddMinutes(-10.0)
        let twoMinutesFromNow = now.AddMinutes(2.0)

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

                ctx.SubmitUpdates()

                FeedUrl job.FeedUrl
            )

        updatedUrls
    )


let private updateJob (FeedUrl url) updateFunction =
    dataAccessOperation (fun ctx ->
        let updatesCount =
            query {
                for job in ctx.Public.FeedJobs do
                where (job.FeedUrl = url)
                take 1
            }
            |> Seq.map updateFunction
            |> Seq.length

        ctx.SubmitUpdates()
        updatesCount
    )


let complete url: AsyncResult<int> =
    let setCompleted (entity: FeedJobEntity) =
        entity.CompletedAt <- Some DateTime.UtcNow
        entity.LockedUntil <- None

    updateJob url setCompleted


let fail url (JobFailure message): AsyncResult<int> =
    let setFailed (entity: FeedJobEntity) =
        entity.LastFailedAt <- Some DateTime.UtcNow
        entity.LastFailure <- message
        entity.LockedUntil <- None

    updateJob url setFailed
