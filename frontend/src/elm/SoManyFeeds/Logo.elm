module SoManyFeeds.Logo exposing (view)

import Html exposing (Html)
import Svg exposing (..)
import Svg.Attributes exposing (..)


view : Html nothing
view =
    svg [ viewBox "0 0 900 200" ]
        [ circle
            [ cx "668"
            , cy "92"
            , r "140"
            , fill "#00BCD4"
            ]
            []
        , text_
            [ x "0"
            , y "160"
            , fontFamily "Merriweather Sans"
            , fontSize "200"
            , fontWeight "400"
            , fill "#FFFFFF"
            ]
            [ text "damo.io" ]
        ]
