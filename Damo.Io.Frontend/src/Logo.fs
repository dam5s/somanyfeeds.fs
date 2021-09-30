[<RequireQualifiedAccess>]
module DamoIOFrontend.Logo

open Fable.React
open Fable.React.Props

let private attr name value = HTMLAttr.Custom(name, value)

let view: ReactElement =
    svg [ attr "viewBox" "0 0 900 200" ]
        [ circle [ Cx 668
                   Cy 92
                   R 140
                   attr "fill" "#00bcd4"
                 ]
                 []
          text [ X 0
                 Y 160
                 attr "font-size" "200"
                 attr "font-weight" "400"
                 attr "fill" "#fff"
               ]
               [ str "damo.io" ]
        ]
