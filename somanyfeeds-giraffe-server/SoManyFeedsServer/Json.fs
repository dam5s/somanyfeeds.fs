module SoManyFeedsServer.Json

open Microsoft.AspNetCore.Http
open System.Text
open Giraffe


let private jsonResponse status (json: string) =
    setHttpHeader "Content-Type" "application/json" >=> setStatusCode status >=> setBody (Encoding.UTF8.GetBytes json)

let objectResponse status object: HttpHandler =
    fun next (ctx: HttpContext) ->
        let json = ctx.GetJsonSerializer().SerializeToString(object)
        jsonResponse status json next ctx

let serverErrorResponse message: HttpHandler =
    objectResponse 500
        {| error = "An error occured"
           message = message |}
