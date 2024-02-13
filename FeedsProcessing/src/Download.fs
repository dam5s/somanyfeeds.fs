module FeedsProcessing.Download


type Url = Url of string

[<RequireQualifiedAccess>]
module Url =
    let value (Url url) = url

type Download = { Url: Url; Content: string }

type DownloadResult = AsyncResult<Download>
