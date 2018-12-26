module FeedsProcessing.DataGateway

open System
open System.Web
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

open FeedsProcessing.Feeds
open FeedsProcessing.Download


type private BasicAuthHeader = BasicAuthHeader of string
type private BearerToken = BearerToken of string

let private consumerKey = Environment.GetEnvironmentVariable "TWITTER_CONSUMER_API_KEY"
let private consumerSecret = Environment.GetEnvironmentVariable "TWITTER_CONSUMER_SECRET"
let private urlEncode (value : string) : string = HttpUtility.UrlEncode value
let private base64encode (value : string) : string = Convert.ToBase64String (Text.Encoding.UTF8.GetBytes value)
let private bearerTokenHeader (BearerToken s) : string = sprintf "Bearer %s" s

let private basicAuthHeader (username : string) (password : string) : BasicAuthHeader =
    let credentials = sprintf "%s:%s"  username password
    BasicAuthHeader <| sprintf "Basic %s" (base64encode credentials)


let private parseToken (jsonString : string) : Result<BearerToken, string> =
    try
        let responseJson = JsonValue.Parse jsonString
        let accessTokenOption = responseJson.TryGetProperty "access_token"
                                    |> Option.map (fun p -> p.AsString ())

        match accessTokenOption with
        | None ->
            Error <| sprintf "Could not parse access_token from json %s" jsonString
        | Some accessToken ->
            Ok <| BearerToken accessToken
    with
    | ex ->
        printfn "Could not parse token json\n\n%s\n\nGot exception %s" jsonString (ex.ToString ())
        Error "Could not parse token json"


let private requestToken (BasicAuthHeader authHeader) : Result<BearerToken, string> =
    try
        let responseString = Http.RequestString
                                ( "https://api.twitter.com/oauth2/token",
                                  httpMethod = "POST",
                                  body = HttpRequestBody.TextRequest "grant_type=client_credentials",
                                  headers = [
                                      Authorization authHeader
                                      ContentType "application/x-www-form-urlencoded;charset=UTF-8"
                                  ]
                                )
        parseToken responseString

    with
    | ex ->
        printfn "There was an error requesting the token. %s" (ex.ToString ())
        Error <| sprintf "There was an error requesting the token. %s" (ex.ToString ())



let private requestTweets (TwitterHandle handle) (token : BearerToken) : DownloadResult =
    try
        Http.RequestString
            ( "https://api.twitter.com/1.1/statuses/user_timeline.json",
              httpMethod = "GET",
              query = [
                  "screen_name", handle
                  "count", "60"
              ],
              headers = [ Authorization <| bearerTokenHeader token ]
            )
            |> DownloadedFeed
            |> Result.Ok

    with
    | ex ->
        printfn "There was an error requesting the token. %s" (ex.ToString ())
        Error <| sprintf "There was an error requesting the token. %s" ex.Message


let downloadTwitterTimeline (handle : TwitterHandle) : DownloadResult =
    let auth = basicAuthHeader (urlEncode consumerKey) (urlEncode consumerSecret)
    Result.bind (requestTweets handle) (requestToken auth)


let downloadFeed (FeedUrl url) : DownloadResult =
    try
        url
            |> Http.RequestString
            |> DownloadedFeed
            |> Result.Ok
    with
    | ex ->
        printfn "There was an error downloading the feed at url %s" url
        Error <| sprintf "There was an error downloading the feed. %s" ex.Message
