module SoManyFeeds.ArticlesProcessing
open SoManyFeeds.ArticlesData
open SoManyFeeds.Feeds


type ProcessingResult =
    | Ok of Record list
    | CannotProcess
    | ProcessingError of string

type Processor = Feed -> ProcessingResult


let private resultToList (result: ProcessingResult): Record list =
    match result with
    | Ok records -> records
    | CannotProcess -> []
    | ProcessingError _ -> []


let private feedToResults (processors: Processor list) (feed: Feed) : ProcessingResult list =
    List.map (fun p -> p(feed)) processors


let processFeeds
    (updateAll : Record list -> unit)
    (processors : Processor list)
    (feeds : Feed list) =

    let newRecords =
        feeds
            |> List.collect (feedToResults processors)
            |> List.collect resultToList

    updateAll newRecords
