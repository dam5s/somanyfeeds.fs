module Server.SslHandler

open Suave
open Suave.Redirection
open Suave.Filters
open Suave.Operators


let private httpsUrlOf (request: HttpRequest) : string =
    request.url.ToString().Replace("http://", "https://")


let private requiresHttps : WebPart =
    fun ctx ->
        ctx.request.header "X-Forwarded-Proto"
            |> Option.ofChoice
            |> Option.map (fun s -> s.ToUpper ())
            |> function
                | Some "HTTP" -> Some ctx
                | _ -> None
            |> async.Return


let private redirectToHttps : WebPart =
    fun ctx ->
        redirect (httpsUrlOf ctx.request) ctx


let enforceSsl : WebPart =
    requiresHttps >=> redirectToHttps
