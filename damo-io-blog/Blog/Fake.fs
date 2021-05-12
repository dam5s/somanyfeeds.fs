module Blog.Fake

[<RequireQualifiedAccess>]
module Fake =
    open Fake.Core

    let private executionContext args =
        let fakeArgs =
            match args with
            | [] -> []
            | x :: _ -> [ "--target"; x ]

        Context.FakeExecutionContext.Create false "Program.fs" fakeArgs

    let setup args f =
        use ctx = executionContext (Array.toList args)
        Context.setExecutionContext (Context.RuntimeContext.Fake ctx)
        f ()

    let newTask name f =
        Target.create name f

    let defaultTask name =
        Target.runOrDefault name
