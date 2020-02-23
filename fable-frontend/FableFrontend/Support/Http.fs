module FableFrontend.Support.Http

open Elmish
open Fable.SimpleHttp
open Fable.SimpleJson


type RequestError =
    | ApiError
    | ParseError of string
    | CmdError


[<RequireQualifiedAccess>]
module Cmd =
    let ofRequest
        (func: 'input -> Async<Result<'a, RequestError>>)
        (input: 'input)
        (msg: Result<'a, RequestError> -> 'msg): Cmd<'msg> =

        Cmd.OfAsync.either func input msg (fun _ -> msg (Error CmdError))


[<RequireQualifiedAccess>]
module RequestError =
    let userMessage err =
        match err with
        | ApiError -> "There was a server error."
        | ParseError _ -> "Failed to parse the response from the server."
        | CmdError -> "There was a connection error."


[<RequireQualifiedAccess>]
module HttpRequest =
    let post path =
        path
        |> Http.request
        |> Http.method POST

    let postJson path body =
        let json =
            body
            |> Json.stringify
            |> BodyContent.Text

        post path
        |> Http.headers [ Headers.contentType "application/json" ]
        |> Http.content json

    let delete path =
        path
        |> Http.request
        |> Http.method DELETE

    let get path =
        path
        |> Http.request
        |> Http.method GET


[<RequireQualifiedAccess>]
module HttpResponse =
    let inline parse<'a> (response: HttpResponse): Result<'a, RequestError> =
        response.responseText
        |> Json.tryParseAs<'a>
        |> Result.mapError ParseError
