module Git

open System
open Fake.IO
open Fake.Core
open Fake.Core.Context


type GitRepository =
    GitRepository of string

type Sha =
    Sha of string

let url (GitRepository value) = value

let sha (Sha value) = value


let private listRemotesProcInfo (GitRepository url) (procInfo : ProcStartInfo) =
    { procInfo with
        FileName = "git"
        Arguments = String.Format("ls-remote --heads {0}", url)
    }


let private runProcess procInfoSetter =
    Process.execWithResult procInfoSetter (TimeSpan.FromSeconds 3.)


let private toShaResult (option : Option<string>) : Result<Sha, string> =
    match option with
    | Some value -> Ok <| Sha value
    | None -> Error "Sha not found"


let latestSha (gitRepo : GitRepository) : Result<Sha, string> =
    use ctxt = FakeExecutionContext.Create false "Git.fs" []
    Context.setExecutionContext (Context.RuntimeContext.Fake ctxt)

    let processResult = listRemotesProcInfo gitRepo |> runProcess

    if processResult.OK then
        processResult.Messages
            |> List.filter (fun m -> m.EndsWith("	refs/heads/master"))
            |> List.map (fun m -> (m.Replace("	refs/heads/master", "")))
            |> List.tryHead
            |> toShaResult
    else
        Error "There was an error fetching the SHA of the git repo"
