module Server.FeedsProcessing.Download


type Url = Url of string

type DownloadedFeed = DownloadedFeed of string

type DownloadResult = Result<DownloadedFeed, string>

let urlString (Url u) = u

let downloadedString (DownloadedFeed f) = f
