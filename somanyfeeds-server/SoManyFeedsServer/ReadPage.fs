module SoManyFeedsServer.ReadPage

open Suave
open Suave.DotLiquid


type ReadViewModel =
    { UserName : string
    }


let page (user : Authentication.User) : WebPart =
    page "read.html.liquid" { UserName = user.Name }
