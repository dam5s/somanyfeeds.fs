[<RequireQualifiedAccess>]
module DamoIoServer.LogoTemplate

open Giraffe.GiraffeViewEngine

let private svg = tag "svg"
let private circle = tag "circle"
let private text = tag "text"

let render: XmlNode =
    svg [ attr "viewBox" "0 0 900 200" ]
        [ circle [ attr "cx" "668"
                   attr "cy" "92"
                   attr "r" "140"
                   attr "fill" "#00bcd4"
                 ]
                 []
          text [ attr "x" "0"
                 attr "y" "160"
                 attr "font-size" "200"
                 attr "font-weight" "400"
                 attr "fill" "#fff"
               ]
               [ str "damo.io" ]
        ]
