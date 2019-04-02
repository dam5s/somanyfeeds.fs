module FeedsProcessing.DataGateway

open System
open System.Web
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

open FeedsProcessing.Feeds
open FeedsProcessing.Download


type private BasicAuthHeader = BasicAuthHeader of string
type private BearerToken = BearerToken of string

let private urlEncode (value : string) : string = HttpUtility.UrlEncode value
let private base64encode (value : string) : string = Convert.ToBase64String (Text.Encoding.UTF8.GetBytes value)
let private bearerTokenHeader (BearerToken s) : string = sprintf "Bearer %s" s

let private basicAuthHeader (username : string) (password : string) : BasicAuthHeader =
    sprintf "%s:%s" username password
    |> base64encode
    |> sprintf "Basic %s"
    |> BasicAuthHeader


let private parseToken (jsonString : string) : Result<BearerToken, string> =
    unsafeOperation "Parse token json" { return! fun _ ->
        let responseJson = JsonValue.Parse jsonString
        let accessTokenOption =
            responseJson.TryGetProperty "access_token"
            |> Option.map (fun p -> p.AsString ())

        match accessTokenOption with
        | None ->
            jsonString
            |> sprintf "Could not parse access_token from json %s"
            |> Error
        | Some accessToken -> Ok (BearerToken accessToken)
    }


let private requestToken (BasicAuthHeader authHeader) : Result<BearerToken, string> =
    unsafeOperation "Request token" { return! fun _ ->
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
    }


let private requestTweets (TwitterHandle handle) (token : BearerToken) : DownloadResult =
    unsafeOperation "Request tweets" { return fun _ ->
        Http.RequestString
            ( "https://api.twitter.com/1.1/statuses/user_timeline.json",
              httpMethod = "GET",
              query = [
                  "screen_name", handle
                  "count", "60"
              ],
              headers = [ Authorization (bearerTokenHeader token) ]
            )
            |> DownloadedFeed
    }


let downloadTwitterTimeline (consumerKey : string) (consumerSecret : string) (handle : TwitterHandle) : DownloadResult =
    let auth = basicAuthHeader (urlEncode consumerKey) (urlEncode consumerSecret)
    Result.bind (requestTweets handle) (requestToken auth)


let downloadFeed (FeedUrl url) : DownloadResult =
    unsafeOperation "Download feed" { return fun _ ->
         url
         |> Http.RequestString
         |> DownloadedFeed
    }
