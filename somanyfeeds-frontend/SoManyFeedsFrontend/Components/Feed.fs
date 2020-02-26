module SoManyFeedsFrontend.Components.Feed

open Fable.Core
open SoManyFeedsFrontend.Support
open SoManyFeedsFrontend.Support.Http
open Result.Operators

type Feed =
    { Id: int64
      Name: string
      Url: string }

[<RequireQualifiedAccess>]
module Feed =
    type Fields =
        { Name: string
          Url: string }

    let emptyFields =
        { Name = ""
          Url = "" }

    type Json =
        { id: int64
          name: string
          url: string }

    let fromJson json =
        { Id = json.id
          Name = json.name
          Url = json.url }

    let decoder (obj: JS.Object) =
        (fun id name url -> { Id = id; Name = name; Url = url })
            <!> (Json.property "id" obj)
            <*> (Json.property "name" obj)
            <*> (Json.property "url" obj)

    let createRequest (fields: Fields) =
        HttpRequest.postJson "/api/feeds" {|name = fields.Name; url = fields.Url|}

    let deleteRequest (feed: Feed) =
        HttpRequest.delete (sprintf "/api/feeds/%d" feed.Id)
