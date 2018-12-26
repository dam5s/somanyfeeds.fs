module FeedsProcessing.Article

open System


type Article =
    { Title : string option
      Link : string option
      Content : string
      Date : DateTimeOffset option
    }
