module SoManyFeedsFrontend.Components.Feed

open Fable.Core
open Fable.SimpleHttp
open Result.Operators
open SoManyFeedsFrontend.Support
open SoManyFeedsFrontend.Support.Http
open SoManyFeedsFrontend.Components.Search

type Feed =
    { Id: int64
      Name: string
      Url: string }

[<RequireQualifiedAccess>]
module Feed =
    type Json =
        { id: int64
          name: string
          url: string }

    let fromJson json =
        { Id = int64 json.id
          Name = json.name
          Url = json.url }

    let private decoder (obj: JS.Object) =
        (fun id name url -> { Id = id; Name = name; Url = url })
            <!> (Json.property "id" obj |> Result.map int64)
            <*> (Json.property "name" obj)
            <*> (Json.property "url" obj)

    let private createRequest (result: SearchResult) =
        HttpRequest.postJson "/api/feeds" {|name = result.Name; url = result.Url|}

    let private deleteRequest (feed: Feed) =
        HttpRequest.delete $"/api/feeds/%d{feed.Id}"

    let sendCreateRequest result =
        async {
            let! response = result
                            |> createRequest
                            |> Http.send

            return if response.statusCode <> 201
                   then Error ApiError
                   else response |> HttpResponse.parse decoder
        }

    let sendDeleteRequest feed =
        async {
            let! response = feed
                            |> deleteRequest
                            |> Http.send

            return if response.statusCode <> 204
                   then Error ApiError
                   else Ok()
        }
