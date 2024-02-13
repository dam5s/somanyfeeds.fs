module Program

open Fake.Core

Fake.initialize ()

Target.create "clean" DotNet.clean
Target.create "build" DotNet.build
Target.create "test" DotNet.test
Target.create "release" (DotNet.release "Damo.Io.Server")

"build" |> Target.mustRunAfter "clean"
"test" |> Target.dependsOn [ "build" ]
"release" |> Target.dependsOn [ "test"; "build"; "clean" ]

Deploy.loadTasks ()

[<EntryPoint>]
let main args =
    Fake.runWithDefault "release" args
