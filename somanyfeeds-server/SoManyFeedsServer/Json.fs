module SoManyFeedsServer.Json

open Chiron
open Chiron.Operators
open Suave
open Suave.Writers
open Suave.Operators
open Suave.Response


let serializeObject (encoder : 'a -> Json<unit>) (record : 'a) : string =
    record
    |> Json.serializeWith encoder
    |> Json.format


let serializeList (encoder : 'a -> Json<unit>) (records : 'a list) : string =
    records
    |> List.map (Json.serializeWith encoder)
    |> Json.Array
    |> Json.format


let jsonResponse (status : HttpCode) (json : string) : WebPart =
    setMimeType "application/json"
        >=> response status (UTF8.bytes json)


let private decodeToResult (decoder : Json -> JsonResult<'a> * Json) (json: Json) : Result<'a, string> =
    decoder json
    |> fst
    |> function | Value value -> Ok value
                | Error msg -> Result.Error msg


let private deserializationErrorJson (message : string) : string =
    let encoder message =
        Json.write "error" "Json deserialization error"
        *> Json.write "message" message

    serializeObject encoder message


let deserializeBody (decoder : Json -> JsonResult<'a> * Json) (next : 'a -> WebPart) : WebPart =
    request (fun r ->
        r.rawForm
        |> UTF8.toString
        |> Json.tryParse
        |> Choice.toResult
        |> Result.bind (decodeToResult decoder)
        |> function | Ok value -> next value
                    | Result.Error msg -> jsonResponse HTTP_400 (deserializationErrorJson msg)
    )


let private jsonError (message : string) : Json<unit> =
    Json.write "error" "An error occured"
    *> Json.write "message" message


let serverError (message : string) =
    message
    |> serializeObject jsonError
    |> jsonResponse HTTP_500
