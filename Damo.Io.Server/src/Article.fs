module DamoIoServer.Article

open Time
open DamoIoServer.Source

type Article =
    { Title: string option
      Link: string option
      Content: string
      Date: Posix option
      Source: Source
    }
