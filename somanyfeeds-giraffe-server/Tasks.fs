module Tasks


type private Task =
    { Name: string
      Job: unit -> unit }

let mutable private tasks: Task list = []

let private defineTask name job =
    tasks <- List.append tasks [ { Name = name; Job = job } ]


let run name =
    tasks
    |> List.tryFind (fun t -> t.Name = name)
    |> Option.map (fun t -> t.Job())
