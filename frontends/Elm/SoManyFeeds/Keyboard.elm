module SoManyFeeds.Keyboard exposing (isEscape)

import Keyboard exposing (Key, RawKey)


isEscape : RawKey -> Bool
isEscape rk =
    isKey rk Keyboard.Escape


isKey : RawKey -> Key -> Bool
isKey rk ek =
    rk
        |> Keyboard.anyKey
        |> Maybe.map ((==) ek)
        |> Maybe.withDefault False
