module SoManyFeedsFrontend.Components.Search

open Fable.Core
open Fable.SimpleHttp
open SoManyFeedsFrontend.Support.Dialog
open SoManyFeedsFrontend.Support.Http
open SoManyFeedsFrontend.Support.RemoteData
open Result.Operators
open SoManyFeedsFrontend.Support

type SearchResult =
    { Name: string
      Description: string
      Url: string }

[<RequireQualifiedAccess>]
module SearchResult =

    let updateName name (result: SearchResult) =
        { result with Name = name }

[<RequireQualifiedAccess>]
module Search =
    type Form =
        { Text: string
          Results: RemoteData<SearchResult list>
          SubscribeDialog: Dialog<SearchResult> }

    let initForm text =
        { Text = text
          Results = NotLoaded
          SubscribeDialog = Initial }

    let private request query =
        query
        |> Http.urlEncode
        |> sprintf "/api/search/%s"
        |> HttpRequest.post

    let private feedDecoder (obj: JS.Object): Result<SearchResult, string> =
        (fun name desc url -> { Name = name; Description = desc; Url = url })
            <!> (Json.property "name" obj)
            <*> (Json.property "description" obj)
            <*> (Json.property "url" obj)

    let sendRequest query: Async<Result<SearchResult list, RequestError>> =
        async {
            let! response = Http.send (request query)
            let decoder = Json.list feedDecoder

            return if response.statusCode <> 200
                   then Error ApiError
                   else response |> HttpResponse.parse decoder
        }
