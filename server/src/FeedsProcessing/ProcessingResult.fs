module Server.FeedsProcessing.ProcessingResult

open Server.ArticlesData


type ProcessingResult = Result<Record list, string>
