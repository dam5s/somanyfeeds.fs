module Server.FeedsProcessing.Download


type DownloadedFeed = DownloadedFeed of string

type DownloadResult = Result<DownloadedFeed, string>


let downloadedString (DownloadedFeed f) = f
