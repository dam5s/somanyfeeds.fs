module Program

open Fake.Core

Fake.initialize ()

Target.create "clean" DotNet.clean
Target.create "build" DotNet.build
Target.create "test" DotNet.test
Target.create "release" (DotNet.release "Damo.Io.Server")

Target.create "format" Fantomas.format
Target.create "lint" Fantomas.check

"build" |> Target.mustRunAfter "clean"
"test" |> Target.dependsOn [ "build" ]
"release" |> Target.dependsOn [ "clean"; "lint"; "test"; "build" ]

Deploy.loadTasks ()

[<EntryPoint>]
let main args = Fake.runWithDefault "release" args
