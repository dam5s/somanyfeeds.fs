module FeedsProcessing.ProcessingResult

open FeedsProcessing.Article


type ProcessingResult = Result<Article list, string>
