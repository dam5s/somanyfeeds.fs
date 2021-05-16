module FeedsProcessingTests.DownloadSupport

open FeedsProcessing.Download
open System.IO


[<RequireQualifiedAccess>]
module Download =
    let fromFilePath path =
        { Url = Url $"file://%s{path}"
          Content = File.ReadAllText path }

    let fromContent content =
        { Url = Url "test://content"
          Content = content }
