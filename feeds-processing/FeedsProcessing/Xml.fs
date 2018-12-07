module FeedsProcessing.Xml

open System


let stringToOption text =
    if String.IsNullOrWhiteSpace text
    then None
    else Some text
