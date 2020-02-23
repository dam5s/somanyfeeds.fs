module FableFrontend.Components.Feed

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

    open FableFrontend.Support.Http

    let createRequest (fields: Fields) =
        HttpRequest.postJson "/api/feeds" fields

    let deleteRequest (feed: Feed) =
        HttpRequest.delete (sprintf "/api/feeds/%d" feed.Id)
