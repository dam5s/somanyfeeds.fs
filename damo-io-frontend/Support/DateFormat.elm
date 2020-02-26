module Support.DateFormat exposing (tryFormat)

import DateFormat exposing (..)
import Time


tryFormat : Maybe Time.Zone -> Time.Posix -> String
tryFormat maybeTimeZone posix =
    case maybeTimeZone of
        Just timeZone ->
            format timeZone posix

        Nothing ->
            ""


format : Time.Zone -> Time.Posix -> String
format =
    DateFormat.format
        [ monthNameFull
        , text " "
        , dayOfMonthSuffix
        , text " '"
        , yearNumberLastTwo
        , text " @ "
        , hourMilitaryNumber
        , text ":"
        , minuteFixed
        ]
