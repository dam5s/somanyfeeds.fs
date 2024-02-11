[<AutoOpen>]
module Support

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open System.IO

exception ProcessException

module Proc =
    let ensureSuccessExitCode exitCode =
        match exitCode with
        | 0 -> ()
        | _ -> raise ProcessException

module File =
    let write filePath content =
        Directory.GetParent(filePath).Create()
        File.writeString false filePath content

module Fake =
    let initialize () =
        let ctx = Context.FakeExecutionContext.Create false "Program.fs" []
        Context.setExecutionContext (Context.RuntimeContext.Fake ctx)

    let runWithDefault defaultTarget args =
        try
            match args with
            | [| target |] -> Target.runOrDefault target
            | _ -> Target.runOrDefault defaultTarget
            0
        with e ->
            printfn $"%A{e}"
            1

module Target =
    let dependsOn dependencies task =
        task <== dependencies

    let mustRunAfter afterTask beforeTask =
        beforeTask <=? afterTask |> ignore
