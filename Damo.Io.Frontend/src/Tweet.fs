[<RequireQualifiedAccess>]
module DamoIoFrontend.Tweet

open System.Text.RegularExpressions
open Fable.React

let private matchReplace replace (m: Match) =
    match List.ofSeq m.Groups with
    | _ :: leading :: target :: _ -> sprintf "%s%s" leading.Value (replace target.Value)
    | _ -> m.Value

let private regexReplace pattern replace input =
    Regex.Replace(input, pattern, MatchEvaluator(matchReplace replace))

let private createMentionLinks =
    regexReplace
        "(^|\\s)@([A-Za-z0-9_]+)"
        (fun matched -> sprintf "<a href=\"https://twitter.com/%s\">@%s</a>" matched matched)

let private createSimpleLinks =
    regexReplace
        "(^|\\s)(https?://[^\\s]+)"
        (fun matched -> sprintf "<a href=\"%s\">%s</a>" matched matched)

let private createHashTagLinks =
    regexReplace
        "(^|\\s)#([A-Za-z_]+[A-Za-z0-9_]+)"
        (fun matched -> sprintf "<a href=\"https://twitter.com/hashtag/%s\">#%s</a>" matched matched)

let private createLinks =
    createMentionLinks >> createSimpleLinks >> createHashTagLinks

let display (content: string): ReactElement =
    section [ Html.rawInnerHtml (createLinks content) ] []
