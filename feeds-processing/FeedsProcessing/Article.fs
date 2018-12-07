module FeedsProcessing.Article

open System
open FeedsProcessing.Feeds


type Article =
    { Title : string option
      Link : string option
      Content : string
      Date : DateTimeOffset option
    }
