module SoManyFeedsServer.SearchApi

open FSharp.Control
open Giraffe
open Microsoft.AspNetCore.Http

open FeedsProcessing
open FeedsProcessing.Search
open FeedsProcessing.Download
open FeedsProcessing.Xml

let rec private recursiveSearch (url: Url): AsyncSeq<FeedMetadata> =
    asyncSeq {
        let! downloadResult =
            DataGateway.downloadContent url

        let searchResult =
            downloadResult
            |> Result.map search
            |> Result.defaultValue (WebPageMatch [])

        match searchResult with
        | FeedMatch f -> yield f
        | WebPageMatch urls  ->
            for u in urls do
                yield! recursiveSearch u
    }

let private toAsyncResult (a: AsyncSeq<'a>): AsyncResult<'a seq> =
    async {
        let value = AsyncSeq.toBlockingSeq a
        return Ok value
    }

[<RequireQualifiedAccess>]
module Json =

    let metadata (m: FeedMetadata) =
        {| name = m.Title
           description = m.Description
           url = Url.value m.Url |}

let private doSearch query: HttpHandler =
    Url query
    |> recursiveSearch
    |> toAsyncResult
    |> Api.list Json.metadata

let private sanitizeQuery (query: string) =
    if query.StartsWith("http://") then query
    else if query.StartsWith("https://") then query
    else sprintf "https://%s" query

let private queryParamValues name (context: HttpContext) =
    context.Request.Query.Item(name).ToArray()

let search : HttpHandler =
    fun next ctx ->
        let q =
            ctx 
            |> queryParamValues "q"
            |> Array.tryHead 
            |> Option.defaultValue ""

        let query = sanitizeQuery q

        doSearch query next ctx
