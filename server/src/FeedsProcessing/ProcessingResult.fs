module Server.FeedsProcessing.ProcessingResult

open Server.Articles.Data


type ProcessingResult = Result<Record list, string>
