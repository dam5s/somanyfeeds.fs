module Support

open Fake.Core.Context
open Fake.Core.TargetOperators
open Fake.IO
open System.IO


exception ProcessException


let ensureSuccessExitCode (exitCode : int) =
    match exitCode with
    | 0 -> ()
    | _ -> raise ProcessException


let writeToFile (filePath : string) (content : string) =
    Directory.GetParent(filePath).Create()
    File.writeString false filePath content


let fakeExecutionContext (args : string list) =
    let fakeArgs =
        match args with
        | [] -> []
        | x::xs -> [ "--target" ; x ]

    FakeExecutionContext.Create false "Program.fs" fakeArgs


let dependsOn (dependencies : string list) (task : string) =
    task <== dependencies


let mustRunAfter (afterTask : string) (beforeTask : string) =
    beforeTask <=? afterTask |> ignore
