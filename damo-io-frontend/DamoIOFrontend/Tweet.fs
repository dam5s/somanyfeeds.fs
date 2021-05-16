[<RequireQualifiedAccess>]
module DamoIOFrontend.Tweet

open System.Text.RegularExpressions
open Fable.React

let private matchReplace replace (m: Match) =
    match List.ofSeq m.Groups with
    | _ :: leading :: target :: _ -> $"%s{leading.Value}%s{replace target.Value}"
    | _ -> m.Value

let private regexReplace pattern replace input =
    Regex.Replace(input, pattern, MatchEvaluator(matchReplace replace))

let private createMentionLinks =
    regexReplace
        "(^|\\s)@([A-Za-z0-9_]+)"
        (fun matched -> $"<a href=\"https://twitter.com/%s{matched}\">@%s{matched}</a>")

let private createSimpleLinks =
    regexReplace
        "(^|\\s)(https?://[^\\s]+)"
        (fun matched -> $"<a href=\"%s{matched}\">%s{matched}</a>")

let private createHashTagLinks =
    regexReplace
        "(^|\\s)#([A-Za-z_]+[A-Za-z0-9_]+)"
        (fun matched -> $"<a href=\"https://twitter.com/hashtag/%s{matched}\">#%s{matched}</a>")

let private createLinks =
    createMentionLinks >> createSimpleLinks >> createHashTagLinks

let display (content: string): ReactElement =
    section [ Html.rawInnerHtml (createLinks content) ] []
