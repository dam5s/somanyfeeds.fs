module Program

open Fake.Core
open Support


[<EntryPoint>]
let main args =
    use ctxt = fakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    Frontend.loadTasks()

    Target.create "clean" DotNet.clean
    Target.create "build" (fun _ ->
        DotNet.build ()
    )
    Target.create "test" (fun _ ->
        DotNet.test ""
    )
    Target.create "release" (fun _ ->
        DotNet.release "Damo.Io.Server" ()
    )

    "clean" |> dependsOn [ "frontend:clean" ]
    "build" |> dependsOn [ "frontend:build" ]

    "build" |> mustRunAfter "clean"

    "test" |> dependsOn [ "build" ]
    "release" |> dependsOn [ "test"; "build"; "clean" ]

    Deploy.loadTasks()
    Target.runOrDefault "release"

    0
