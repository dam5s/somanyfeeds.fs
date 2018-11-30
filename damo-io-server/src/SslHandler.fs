module server.SslHandler

open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Extensions
open Giraffe


let private getUrlAsHttps (request: HttpRequest) : string =
    UriHelper.GetEncodedUrl(request).Replace("http://", "https://")


let private shouldRedirectToHttps (ctx : HttpContext) : bool =
    ctx.TryGetRequestHeader "X-Forwarded-Proto"
        |> Option.map (fun s -> s.ToUpper())
        |> function
            | Some "HTTP" -> true
            | _ -> false


let enforceSsl (next: HttpFunc) (ctx: HttpContext) : HttpFuncResult =
    if shouldRedirectToHttps ctx
    then (getUrlAsHttps ctx.Request |> redirectTo false) next ctx
    else Task.FromResult None
