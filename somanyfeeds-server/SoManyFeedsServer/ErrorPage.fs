module SoManyFeedsServer.ErrorPage

open Suave
open Suave.ServerErrors
open Suave.DotLiquid


type ErrorViewModel =
    { Message : string
    }


let page (message : string) : WebPart =
    page "error.html.liquid" { Message = message }
