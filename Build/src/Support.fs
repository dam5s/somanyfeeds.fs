module Support

open Fake.Core.Context
open Fake.Core.TargetOperators
open Fake.IO
open System.IO


exception ProcessException


let ensureSuccessExitCode exitCode =
    match exitCode with
    | 0 -> ()
    | _ -> raise ProcessException


let writeToFile filePath content =
    Directory.GetParent(filePath).Create()
    File.writeString false filePath content


let fakeExecutionContext args =
    let fakeArgs =
        match args with
        | [] -> []
        | x :: xs -> [ "--target"; x ]

    FakeExecutionContext.Create false "Program.fs" fakeArgs


let dependsOn dependencies task =
    task <== dependencies


let mustRunAfter afterTask beforeTask =
    beforeTask <=? afterTask |> ignore
