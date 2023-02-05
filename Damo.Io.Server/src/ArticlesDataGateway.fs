module DamoIoServer.ArticlesDataGateway

open DamoIoServer.Sources
open Time


type ArticleRecord =
    { Title: string option
      Link: string option
      Content: string
      Date: Posix option
      Source: SourceType
    }


let mutable private allRecords: ArticleRecord list = []


module Repository =

    let findAll() = allRecords

    let updateAll newRecords =
        allRecords <- newRecords
