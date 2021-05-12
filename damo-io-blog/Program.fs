open Blog.Fake
open Blog.Tasks

[<EntryPoint>]
let main argv =
    Fake.setup argv (fun _ ->
        Fake.newTask "build" Tasks.build
        Fake.defaultTask "build"
    )
    0
