module BuildRunner

open System
open Fake.IO
open Fake.Core
open Fake.Core.Context
open Build


let private runBuildProcInfo (script : DockerScript) (procInfo : ProcStartInfo) =
    { procInfo with
        FileName = "docker"
        Arguments = String.Format ("run {0} ./{1}", script.image, script.scriptName)
    }


let private runBuildProcess procInfoSetter =
    Process.execWithResult procInfoSetter (TimeSpan.FromMinutes 3.)


let private doRun (script : DockerScript) (build : Build) : BuildStatus =
    use ctxt = FakeExecutionContext.Create false "BuildRunner.fs" []
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    let processResult = runBuildProcess (runBuildProcInfo script)
    let log = processResult.Results
                |> List.map (fun r -> r.Message)
                |> String.concat "\n"
                |> BuildLog

    if processResult.OK then
        Successful log
    else
        Failed log


let run (script : DockerScript) (pendingBuild : Build) : Build =
    let build = BuildRepository.upsert { pendingBuild with status = Running }

    printfn "Running build #%d with SHA %s" build.id (Git.sha build.sha)

    let finalStatus = doRun script build

    printfn "Done running build #%d" build.id

    BuildRepository.upsert { build with status = finalStatus }
