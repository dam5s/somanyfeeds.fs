module BuildHandlers

open Giraffe
open GiraffeViewEngine
open Microsoft.AspNetCore.Http
open HttpSupport
open System
open Build


module private View =
    type StatusViewModel =
        { name : string
        ; message : string option
        ; log : string option
        }

    type ViewModel =
        { id : int
        ; sha : string
        ; status : StatusViewModel
        }

    let presentStatus (status : BuildStatus) : StatusViewModel =
        match status with
        | Pending -> { name = "Pending" ; message = None ; log = None }
        | Running -> { name = "Running" ; message = None ; log = None }
        | Successful (BuildLog log) -> { name = "Successful" ; message = None ; log = Some log }
        | Failed (BuildLog log) -> { name = "Failed" ; message = None ; log = Some log }


    let present (build : Build) : ViewModel =
        { id = build.id
        ; sha = Git.sha build.sha
        ; status = presentStatus build.status
        }


    let buildView (build : ViewModel) : XmlNode =
        li [] [ rawText <| String.Format ("Build #{0} - Status {1}", build.id, build.status.name) ]


    let listView (builds : ViewModel list) : XmlNode list =
        [ ul [] <| List.map buildView builds ]


let list
    (layout : XmlNode list -> XmlNode)
    (findBuilds : unit -> Build list) =

    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let builds = findBuilds () |> List.map View.present

            return!
                match HttpSupport.accept ctx with
                | Html -> htmlView (View.listView builds |> layout) next ctx
                | Json -> ctx.WriteJsonAsync builds
        }
