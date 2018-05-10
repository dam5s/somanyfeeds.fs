module SoManyFeeds.Source exposing (Source, all, fromString, toString)


type Source
    = About
    | Social
    | Code
    | Blog


all : List Source
all =
    [ About, Social, Code, Blog ]


fromString : String -> Source
fromString value =
    case value of
        "About" ->
            About

        "Social" ->
            Social

        "Code" ->
            Code

        _ ->
            Blog


toString : Source -> String
toString value =
    case value of
        About ->
            "About"

        Social ->
            "Social"

        Code ->
            "Code"

        Blog ->
            "Blog"
