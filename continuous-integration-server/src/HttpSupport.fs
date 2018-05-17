module HttpSupport

open Microsoft.AspNetCore.Http
open Giraffe


type ContentType =
    | Html
    | Json


let accept (ctx : HttpContext) : ContentType =
    match ctx.GetRequestHeader "Accept" with
    | Error _ -> Html
    | Ok header ->
        if header.Contains "application/json" then
            Json
        else
            Html
