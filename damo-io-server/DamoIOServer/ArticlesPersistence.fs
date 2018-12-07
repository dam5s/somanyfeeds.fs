module DamoIOServer.ArticlesPersistence

open System
open DamoIOServer.Sources


type ArticleRecord =
    { Title : string option
      Link : string option
      Content : string
      Date : DateTimeOffset option
      Source : SourceType
    }


let mutable private allRecords : ArticleRecord list = []


module Repository =

    let findAll () : ArticleRecord list = allRecords

    let updateAll (newRecords : ArticleRecord list) = allRecords <- newRecords
