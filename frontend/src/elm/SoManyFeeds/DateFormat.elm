module SoManyFeeds.DateFormat exposing (parseAndFormat)

import DateFormat exposing (..)
import Time


parseAndFormat : Maybe Time.Zone -> Time.Posix -> String
parseAndFormat maybeTimeZone posix =
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
