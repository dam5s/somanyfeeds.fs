module Tasks

open FeedsProcessing
open SoManyFeedsServer
open SoManyFeedsServer.DataSource


type private Task =
    { Name : string
      Job : unit -> unit }

let mutable private tasks: Task list = []

let private defineTask name job =
    tasks <- List.append tasks [{ Name = name ; Job = job }]


module SanitizeArticles =

    type private ArticleRecord = { Id : int64 ; Content : string }

    let private updateArticles (articles : ArticleRecord list) =
        articles
        |> List.map (fun record ->
            DataSource.update
                DataAccess.dataSource
                "update articles set content = @Content where id = @Id"
                [
                Binding ("@Content", Html.sanitize record.Content)
                Binding ("@Id", record.Id)
                ]
            |> Async.RunSynchronously
        )

    defineTask "sanitizeArticles" (fun _ ->
        printfn "Sanitizing articles..."

        let articlesResult =
            DataSource.query
                DataAccess.dataSource
                "select id, content from articles"
                []
                (fun record -> { Id = record.GetInt64 0 ; Content = record.GetString 1 })

        articlesResult
        |> AsyncResult.map updateArticles
        |> Async.RunSynchronously
        |> ignore
    )


let run name =
    tasks
    |> List.tryFind (fun t -> t.Name = name)
    |> Option.map (fun t -> t.Job ())
