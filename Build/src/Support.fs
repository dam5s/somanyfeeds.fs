[<AutoOpen>]
module Support

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open System.IO

exception ProcessException

[<RequireQualifiedAccess>]
type Proc private () =
    static member exec (cmd: string, ?pwd: string) =
        let (program: string, commandLine: string) =
            match Array.toList(cmd.Split " ") with
            | [program] -> program, ""
            | program :: args -> program, String.concat " " args
            | _ -> "", ""
    
        let exitCode: int =
            Process.shellExec
                { Program = program
                  WorkingDir = Option.defaultValue "." pwd
                  CommandLine = commandLine
                  Args = [] }

        if exitCode <> 0
        then raise ProcessException
        else ()

[<RequireQualifiedAccess>]
module File =
    let write filePath content =
        Directory.GetParent(filePath).Create()
        File.writeString false filePath content

[<RequireQualifiedAccess>]
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

[<RequireQualifiedAccess>]
module Target =
    let dependsOn dependencies task =
        task <== dependencies

    let mustRunAfter afterTask beforeTask =
        beforeTask <=? afterTask |> ignore
