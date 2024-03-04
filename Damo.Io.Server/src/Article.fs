module DamoIoServer.Article

open Time

open FeedsProcessing.Article
open DamoIoServer.Source

type MediaRecord = { Url: string; Description: string }

module MediaRecord =
    let ofMedia (media: Media) =
        { Url = media.Url
          Description = media.Description }

type ArticleRecord =
    { Title: string option
      Link: string option
      Content: string
      Media: MediaRecord option
      Date: Posix option
      SourceType: Source
      SourceName: string }
