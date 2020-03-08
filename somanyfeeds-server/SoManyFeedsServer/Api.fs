module SoManyFeedsServer.Api

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open System.Text
open Giraffe


type private Logs = Logs
let private logger = Logger<Logs>()

let private buildJsonResponse status (json: string) =
        setHttpHeader "Content-Type" "application/json"
        >=> setStatusCode status
        >=> setBody (Encoding.UTF8.GetBytes json)

let serializeObject object (ctx: HttpContext) =
    ctx.GetJsonSerializer().SerializeToString(object)

let jsonResponse status object: HttpHandler =
    fun next (ctx: HttpContext) ->
        let json = serializeObject object ctx
        buildJsonResponse status json next ctx

let serverErrorResponse (explanation: Explanation): HttpHandler =
    explanation
    |> logger.Error
    |> fun e ->
        jsonResponse 500
            {| error = "An error occured"
               message = e.Message |}

let private operationWithoutInput (asyncRes: AsyncResult<'record>) (onSuccess: 'record -> HttpHandler): HttpHandler =
    fun next ctx -> task {
        match! asyncRes with
        | Ok value -> return! onSuccess value next ctx
        | Error explanation -> return! serverErrorResponse explanation next ctx
    }

let private operationWithInput (asyncFunc: 'input -> AsyncResult<'record>) (onSuccess: 'record -> HttpHandler): 'input -> HttpHandler =
    fun input next ctx -> task {
        match! asyncFunc input with
        | Ok value -> return! onSuccess value next ctx
        | Error explanation -> return! serverErrorResponse explanation next ctx
    }

let view (jsonMapping: 'record -> 'json) (asyncRes: AsyncResult<'record>): HttpHandler =
    let onSuccess =
        jsonMapping >> jsonResponse 200

    operationWithoutInput asyncRes onSuccess

let list (jsonMapping: 'record -> 'json) (asyncRes: AsyncResult<'record seq>): HttpHandler =
    let onSuccess =
        Seq.map jsonMapping >> jsonResponse 200

    operationWithoutInput asyncRes onSuccess

let create (jsonMapping: 'record -> 'json) (asyncFunc: 'input -> AsyncResult<'record>) =
    let onSuccess =
        jsonMapping >> jsonResponse 201

    operationWithInput asyncFunc onSuccess

let update (jsonMapping: 'record -> 'json) (asyncFunc: 'input -> AsyncResult<'record>) =
    let onSuccess =
        jsonMapping >> jsonResponse 200

    operationWithInput asyncFunc onSuccess

let action (asyncRes: AsyncResult<unit>) =
    let onSuccess =
        jsonResponse 200

    operationWithoutInput asyncRes onSuccess

let delete (asyncFunc: unit -> AsyncResult<unit>) =
    let onSuccess _ =
        setStatusCode 204

    operationWithInput asyncFunc onSuccess ()
