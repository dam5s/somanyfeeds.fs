module SoManyFeedsServer.Json

open Chiron
open Chiron.Operators
open Suave
open Suave.Writers
open Suave.Operators
open Suave.Response

let serializeSimpleMap (map : Map<string, obj>) : string =
    map
    |> Map.map (fun k (value : obj) ->
        match value with
        | null -> Json.Null ()
        | :? Json as x -> x
        | :? bool as x -> Json.Bool x
        | :? int64 as x -> Json.Number (decimal x)
        | :? string as x -> Json.String x
        | _ ->
            eprintfn "Unsupported value for session: %A" value
            Json.Null ()
    )
    |> Json.Object
    |> Json.format


let deserializeSimpleMap (json : string) : Map<string, obj> =
    let convertJsonMap (map : Map<string, Json>) : Map<string, obj> =
        map |> Map.map (fun key (value : Json) ->
            match value with
            | Json.Null _ -> null
            | Json.Bool b -> b :> obj
            | Json.Number n -> int64 n :> obj
            | Json.String s -> s :> obj
            | _ ->
                eprintfn "Unsupported value in session: %A" value
                null
        )

    json
    |> Json.tryParse
    |> Choice.toResult
    |> Result.fold id (fun _ -> Json.Object Map.empty)
    |> function | Json.Object map -> convertJsonMap map
                | _ -> Map.empty


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
