module SoManyFeedsServer.Auth.JWT

open System.Collections.Generic
open System.Text
open JWT
open JWT.Algorithms
open JWT.Serializers

let private algorithm = HMACSHA256Algorithm()
let private serializer = JsonNetSerializer()
let private urlEncoder = JwtBase64UrlEncoder()
let private validator = JwtValidator(serializer, UtcDateTimeProvider())
let private encoder = JwtEncoder(algorithm, serializer, urlEncoder)
let private decoder = JwtDecoder(serializer, validator, urlEncoder, algorithm)


let encode (key: string) (content: IDictionary<string, obj>) =
    encoder.Encode(content, key)
let decode (key: string) (value: string): IDictionary<string, obj> =
    try decoder.DecodeToObject(value, key, true)
    with ex -> dict []
