module SoManyFeedsServer.Json

open Chiron


let serializeObject (encoder : 'a -> Json<unit>) (record : 'a) : string =
    record
    |> Json.serializeWith encoder
    |> Json.format


let serializeList (encoder : 'a -> Json<unit>) (records : 'a list) : string =
    records
    |> List.map (Json.serializeWith encoder)
    |> Json.Array
    |> Json.format
