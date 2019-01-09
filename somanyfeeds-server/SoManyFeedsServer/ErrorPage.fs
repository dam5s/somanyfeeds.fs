module SoManyFeedsServer.ErrorPage

open Suave
open Suave.DotLiquid
open Suave.Operators


type ErrorViewModel =
    { Message : string }


let page (message : string) : WebPart =
    Writers.setStatus HTTP_500 >=> page "error.html.liquid" { Message = message }
