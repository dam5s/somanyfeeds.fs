module FeedsProcessing.DataGateway

open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open FeedsProcessing.Download
open FeedsProcessing.Feeds
open System
open System.Web

type private BasicAuthHeader = BasicAuthHeader of string
type private BearerToken = BearerToken of string

let private urlEncode (value: string): string =
    HttpUtility.UrlEncode value
let private base64encode (value: string): string =
    Convert.ToBase64String(Text.Encoding.UTF8.GetBytes value)
let private bearerTokenHeader (BearerToken s) =
    sprintf "Bearer %s" s

let private basicAuthHeader username password =
    sprintf "%s:%s" username password
    |> base64encode
    |> sprintf "Basic %s"
    |> BasicAuthHeader

let private parseToken jsonString =
    Try.result "Parse token json" (fun _ ->
        let responseJson = JsonValue.Parse jsonString
        let accessTokenOption =
            responseJson.TryGetProperty "access_token"
            |> Option.map (fun p -> p.AsString())

        match accessTokenOption with
        | None ->
            jsonString
            |> sprintf "Could not parse access_token from json %s"
            |> Error.ofMessage
        | Some accessToken -> Ok(BearerToken accessToken)
    )

let private requestToken (BasicAuthHeader authHeader) =
    Try.result "Request token" (fun _ ->
        let responseString = Http.RequestString
                                ("https://api.twitter.com/oauth2/token",
                                  httpMethod = "POST",
                                  body = HttpRequestBody.TextRequest "grant_type=client_credentials",
                                  headers = [
                                      Authorization authHeader
                                      ContentType "application/x-www-form-urlencoded;charset=UTF-8"
                                  ]
                                )
        parseToken responseString
    )

let private requestTweets (TwitterHandle handle) token =
    Try.value "Request tweets" (fun _ ->
        let url = sprintf "https://twitter.com/%s" handle

        let content =
            Http.RequestString
                ("https://api.twitter.com/1.1/statuses/user_timeline.json",
                  httpMethod = "GET",
                  query = [
                      "screen_name", handle
                      "count", "60"
                  ],
                  headers = [ Authorization(bearerTokenHeader token) ]
                )

        { Url = (Url url); Content = content }
    )

let downloadTwitterTimeline consumerKey consumerSecret handle: DownloadResult =
    async {
        let auth = basicAuthHeader (urlEncode consumerKey) (urlEncode consumerSecret)
        return Result.bind (requestTweets handle) (requestToken auth)
    }

let downloadContent (Url url): DownloadResult =
    async {
        return Try.value "Download content" (fun _ ->
            let content =
                Http.RequestString
                    ( url,
                      headers = [ "User-Agent", "somanyfeeds.com" ],
                      responseEncodingOverride = "utf-8" )
            { Url = (Url url); Content = content }
        )
    }
