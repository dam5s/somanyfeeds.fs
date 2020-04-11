module Tasks

open FeedsProcessing
open SoManyFeedsPersistence.DataSource

type private Task =
    { Name: string
      Job: unit -> unit }

let mutable private tasks: Task list = []

let private defineTask name job =
    tasks <- List.append tasks [ { Name = name; Job = job } ]

defineTask "sanitizeDualShockerArticles" (fun _ ->
    let sanitize (e: ArticleEntity) =
        printfn "Sanitizing %s" e.Url
        e.Content <- Html.sanitize e.Content

    dataAccessOperation (fun ctx ->
        query {
            for a in ctx.Public.Articles do
            where (a.FeedUrl = "https://www.dualshockers.com/feed/atom/")
        }
        |> Seq.iter sanitize

        ctx.SubmitUpdates()
    )
    |> Async.RunSynchronously
    |> ignore
)

let run name =
    tasks
    |> List.tryFind (fun t -> t.Name = name)
    |> Option.map (fun t -> t.Job())
