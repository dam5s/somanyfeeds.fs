module DamoIOServer.Articles.Data

open System
open DamoIOServer.Sources


type Record =
    { Title : string option
      Link : string option
      Content : string
      Date : DateTimeOffset option
      Source : SourceType
    }


let mutable private allRecords : Record list = []


module Repository =

    let findAll () : Record list = allRecords

    let updateAll (newRecords : Record list) = allRecords <- newRecords
