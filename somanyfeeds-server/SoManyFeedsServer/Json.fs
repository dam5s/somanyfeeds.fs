module SoManyFeedsServer.Json

open Chiron
open Chiron.Operators
open Suave
open Suave.Writers
open Suave.Operators
open Suave.Response


let private logger = getLogger "somanyfeeds-server.json"


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
            sprintf "Unsupported value for session: %A" value
            |> logError logger
            |> always (Json.Null ())
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
                sprintf "Unsupported value in session: %A" value
                |> logError logger
                |> always null
        )

    json
    |> Json.tryParse
    |> Choice.defaultValue (Json.Object Map.empty)
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


let objectResponse (status : HttpCode) (encoder : 'a -> Json<unit>) (object : 'a) : WebPart =
    object
    |> serializeObject encoder
    |> jsonResponse status



let listResponse (status : HttpCode) (encoder : 'a -> Json<unit>) (list : 'a list) : WebPart =
    list
    |> serializeList encoder
    |> jsonResponse status


let private decodeToChoice (decoder : Json -> JsonResult<'a> * Json) (json: Json) : Choice<'a, string> =
    decoder json
    |> fst
    |> function | Value value -> Choice1Of2 value
                | Error msg -> Choice2Of2 msg


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
        |> Choice.bind (decodeToChoice decoder)
        |> function | Choice1Of2 value -> next value
                    | Choice2Of2 msg -> jsonResponse HTTP_400 (deserializationErrorJson msg)
    )


let private errorEncoder (message : string) : Json<unit> =
    Json.write "error" "An error occured"
    *> Json.write "message" message


let serverErrorResponse (message : string) : WebPart =
    objectResponse HTTP_500 errorEncoder message
