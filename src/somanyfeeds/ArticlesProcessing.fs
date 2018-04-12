module SoManyFeeds.ArticlesProcessing
open SoManyFeeds.ArticlesData
open SoManyFeeds.Feeds


type ProcessingResult = Result<Record list, string>

type Processor = Feed -> ProcessingResult


let private resultToList (result: ProcessingResult): Record list =
    match result with
    | Ok records -> records
    | Error _ -> []


let private feedToResults (processors: Processor list) (feed: Feed) : ProcessingResult list =
    List.map (fun p -> p(feed)) processors


let processFeeds
    (updateAll : Record list -> unit)
    (processors : Processor list)
    (findAllFeeds : unit -> Feed list) =

    let newRecords =
        (findAllFeeds())
            |> List.collect (feedToResults processors)
            |> List.collect resultToList

    updateAll newRecords
