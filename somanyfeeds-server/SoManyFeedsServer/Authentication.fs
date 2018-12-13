module SoManyFeedsServer.Authentication

open Suave
open Suave.Operators


type User =
    { Id : int64
      Name : string
    }

let private setSessionValue (key : string) (value : obj) : WebPart =
    fun ctx ->
        let newState = Map.add key value ctx.userState

        { ctx with userState = newState }
        |> Option.Some
        |> async.Return


let private tryCast (o : obj) : 'T option =
    try
        Some (o :?> 'T)
    with _ ->
        None


let tryGetUser (ctx : HttpContext) : User option =
    ctx.userState
        |> Map.tryFind "user"
        |> Option.bind tryCast


let authenticate (f : User -> WebPart) (_ : HttpRequest) : WebPart =
    let user =
        { Id = int64 1 ; Name = "Damo" }

    setSessionValue "user" user >=> f user
