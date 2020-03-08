module FeedsProcessing.Download


type DownloadedFeed = DownloadedFeed of string

type DownloadResult = Result<DownloadedFeed, Explanation>
