module DamoIO.Tweet exposing (display)

import Html exposing (Attribute, Html, section)
import Regex
import Support.RawHtml as RawHtml


display : String -> Html msg
display content =
    section [] <| RawHtml.fromString (createLinks content)


createLinks : String -> String
createLinks =
    createMentionLinks >> createSimpleLinks >> createHashTagLinks


buildRegex =
    Regex.fromString >> Maybe.withDefault Regex.never


createMentionLinks =
    Regex.replace
        (buildRegex "(^|\\s)@([A-Za-z_]+[A-Za-z0-9_]+)")
        (replaceLink (\s -> "<a href=\"https://twitter.com/" ++ s ++ "\">@" ++ s ++ "</a>"))


createSimpleLinks =
    Regex.replace
        (buildRegex "(^|\\s)(https?://[^\\s]+)")
        (replaceLink (\s -> "<a href=\"" ++ s ++ "\">" ++ s ++ "</a>"))


createHashTagLinks =
    Regex.replace
        (buildRegex "(^|\\s)#([A-Za-z_]+[A-Za-z0-9_]+)")
        (replaceLink (\s -> "<a href=\"https://twitter.com/hashtag/" ++ s ++ "\">#" ++ s ++ "</a>"))


replaceLink : (String -> String) -> Regex.Match -> String
replaceLink linkBuilder match =
    case match.submatches of
        maybeLeading :: maybeTarget :: _ ->
            case maybeTarget of
                Nothing ->
                    match.match

                Just subMatch ->
                    Maybe.withDefault "" maybeLeading ++ linkBuilder subMatch

        _ ->
            match.match
