module SoManyFeedsFrontend.Components.Search

open Fable.Core
open Fable.SimpleHttp
open SoManyFeedsFrontend.Support.Http
open SoManyFeedsFrontend.Support.RemoteData
open Result.Operators
open SoManyFeedsFrontend.Support

type SearchResultStatus =
    | Unsubscribed
    | Subscribing
    | Subscribed

type SearchResult =
    { Name: string
      Description: string
      Url: string
      Status: SearchResultStatus }

[<RequireQualifiedAccess>]
module SearchResult =

    let updateName name (result: SearchResult) =
        { result with Name = name }

    let setStatus status result =
        { result with Status = status }

[<RequireQualifiedAccess>]
module SearchResults =
    let setStatus status result list =
        let isSameResult a b =
            a.Name = b.Name && a.Description = b.Description && a.Url = b.Url

        List.updateIf (isSameResult result) (SearchResult.setStatus status) list

type SearchForm =
    { Text: string
      Results: RemoteData<SearchResult list> }

[<RequireQualifiedAccess>]
module SearchForm =
    let init text =
        { Text = text
          Results = NotLoaded }

    let setStatus status result form =
        { form with Results = form.Results |> RemoteData.map (SearchResults.setStatus status result) }

[<RequireQualifiedAccess>]
module SearchRequest =
    let private request query =
        query
        |> Http.urlEncode
        |> sprintf "/api/search?q=%s"
        |> HttpRequest.post

    let private feedDecoder (obj: JS.Object): Result<SearchResult, string> =
        (fun name desc url -> { Name = name; Description = desc; Url = url; Status = Unsubscribed })
            <!> (Json.property "name" obj)
            <*> (Json.property "description" obj)
            <*> (Json.property "url" obj)

    let send query: Async<Result<SearchResult list, RequestError>> =
        async {
            let! response = Http.send (request query)
            let decoder = Json.list feedDecoder

            return if response.statusCode <> 200
                   then Error ApiError
                   else response |> HttpResponse.parse decoder
        }
