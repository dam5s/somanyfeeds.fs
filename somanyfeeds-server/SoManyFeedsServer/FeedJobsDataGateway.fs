module FeedJobsDataGateway

open FeedsProcessing.Feeds
open SoManyFeedsServer.DataSource


type JobFailure =
    JobFailure of message:string


let createMissing (dataSource : DataSource) : AsyncResult<int> =
    update
        dataSource
        """
        insert into feed_jobs (feed_url)
        select url from feeds where url not in (select feed_url from feed_jobs)
        """
        []


let startSome (dataSource : DataSource) (howMany : int): AsyncResult<List<FeedUrl>> =
    query
        dataSource
        """
        update feed_jobs
        set started_at   = now(),
            locked_until = now() + interval '2 minutes'
        where feed_url IN (
            select feed_url
            from feed_jobs
            where (locked_until is null or locked_until < now())
              and (completed_at is null or completed_at < now() - interval '10 minutes')
            limit @HowMany
            for update skip locked
        )
        returning feed_url
        """
        [ Binding ("HowMany", howMany) ]
        (fun record -> FeedUrl <| record.GetString 0)


let complete (dataSource : DataSource) (FeedUrl url): AsyncResult<int> =
    update
        dataSource
        """
        update feed_jobs
        set completed_at = now(), locked_until = null
        where feed_url = @FeedUrl
        """
        [ Binding ("FeedUrl", url) ]


let fail (dataSource : DataSource) (FeedUrl url) (JobFailure message): AsyncResult<int> =
    update
        dataSource
        """
        update feed_jobs
        set last_failed_at = now(), last_failure = @Message, locked_until = null
        where feed_url = @FeedUrl
        """
        [ Binding ("FeedUrl", url)
          Binding ("Message", message)
        ]
