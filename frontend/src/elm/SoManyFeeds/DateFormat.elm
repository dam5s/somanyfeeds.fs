module SoManyFeeds.DateFormat exposing (parseAndFormat)

import Date


parseAndFormat : String -> String
parseAndFormat zuluTime =
    case Date.fromString zuluTime of
        Ok date ->
            format date

        Err message ->
            zuluTime


format : Date.Date -> String
format date =
    toString (Date.month date)
        ++ " "
        ++ toString (Date.day date)
        ++ " "
        ++ toString (Date.year date)
        ++ " @ "
        ++ toString (Date.hour date)
        ++ ":"
        ++ withZeroPadding (toString (Date.minute date))


withZeroPadding : String -> String
withZeroPadding text =
    if String.length text > 1 then
        text
    else
        "0" ++ text
