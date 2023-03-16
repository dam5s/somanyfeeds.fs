module Program

open Fake.Core
open Support

[<EntryPoint>]
let main args =
    use context = fakeExecutionContext (Array.toList args)
    Context.setExecutionContext (Context.RuntimeContext.Fake context)

    Target.create "clean" DotNet.clean
    Target.create "build" DotNet.build
    Target.create "test" (fun _ -> DotNet.test "")
    Target.create "release" (DotNet.release "Damo.Io.Server")

    "build" |> mustRunAfter "clean"
    "test" |> dependsOn [ "build" ]
    "release" |> dependsOn [ "test"; "build"; "clean" ]

    Deploy.loadTasks()
    Target.runOrDefault "release"

    0
