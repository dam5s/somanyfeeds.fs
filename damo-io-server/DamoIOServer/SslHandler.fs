module DamoIOServer.SslHandler

open System
open Suave
open Suave.Redirection
open Suave.Operators


let private httpsUrlOf (request: HttpRequest) : string =
    let builder = new UriBuilder ()
    builder.Scheme <- "https"
    builder.Host <- request.host
    builder.Path <- request.path
    builder.Uri.ToString ()


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
