module DamoIOServer.FeedsProcessing.ProcessingResult

open DamoIOServer.Articles.Data


type ProcessingResult = Result<Record list, string>
