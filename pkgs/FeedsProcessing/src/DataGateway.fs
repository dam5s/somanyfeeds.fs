module FeedsProcessing.DataGateway

open System.Threading.Tasks
open FSharp.Data
open FeedsProcessing.Download
open Microsoft.Extensions.Logging

[<RequireQualifiedAccess>]
type DataGateway(logger: ILogger<DataGateway>) =
    let download (Url url) =
        try
            let content =
                Http.RequestString(
                    url,
                    headers = [ "User-Agent", "somanyfeeds.com" ],
                    responseEncodingOverride = "utf-8"
                )

            Ok { Url = (Url url); Content = content }
        with ex ->
            logger.LogError(ex, "Error downloading {url}", url)
            Error [ ex ]

    member _.DownloadAsync url : TaskResult<Download> = Task.Run(fun () -> download url)
