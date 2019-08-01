module DamoIOServer.ArticlesDataGateway

open Time
open DamoIOServer.Sources


type ArticleRecord =
    { Title : string option
      Link : string option
      Content : string
      Date : Posix option
      Source : SourceType
    }


let mutable private allRecords : ArticleRecord list = []


module Repository =

    let findAll () = allRecords

    let updateAll newRecords =
        allRecords <- newRecords
