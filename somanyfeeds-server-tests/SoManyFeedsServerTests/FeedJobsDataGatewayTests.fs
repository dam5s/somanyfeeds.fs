module ``FeedJobsDataGateway tests``

open System
open NUnit.Framework
open FsUnitTyped
open FeedsProcessing.Feeds
open SoManyFeedsServer
open SoManyFeedsServer.DataSource


[<Test>]
let ``standard background processing flow`` () =
    setTestDbConnectionString ()

    executeSql "delete from feed_jobs"
    executeSql "delete from feeds"
    executeSql "delete from users"
    executeSql "insert into users (id, email, name, password_hash) values (10, 'john@example.com', 'Johnny', '')"
    executeSql """
        insert into feeds (id, user_id, name, url) values
        (101, 10, 'My Feed #1', 'http://example.com/my-feeds/1'),
        (102, 10, 'My Feed #2', 'http://example.com/my-feeds/2'),
        (103, 10, 'My Feed #3', 'http://example.com/my-feeds/3')
    """


    FeedJobsDataGateway.createMissing
    |> Async.RunSynchronously
    |> ignore


    let urls = queryDataContext (fun ctx ->
        query { for job in ctx.Public.FeedJobs do
                sortBy job.FeedUrl
                select job.FeedUrl
              })
    urls |> shouldEqual [ "http://example.com/my-feeds/1"
                          "http://example.com/my-feeds/2"
                          "http://example.com/my-feeds/3"
                        ]


    let started = FeedJobsDataGateway.startSome 2
                    |> Async.RunSynchronously
                    |> Result.defaultValue Seq.empty

    started
    |> Seq.sort
    |> Seq.toList
    |> shouldEqual [ FeedUrl "http://example.com/my-feeds/1"
                     FeedUrl "http://example.com/my-feeds/2"
                   ]


    let jobs = queryDataContext (fun ctx -> query { for job in ctx.Public.FeedJobs do select job })

    jobs
    |> List.filter (fun job -> job.StartedAt.IsSome)
    |> List.length
    |> shouldEqual 2
