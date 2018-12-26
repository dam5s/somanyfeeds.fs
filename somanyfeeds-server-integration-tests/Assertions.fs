module Assertions

open canopy.classic

let expectToFind (cssSelector : string) (containingText : string) =
    let message = sprintf "Expected to find css '%s' containing text '%s'." cssSelector containingText

    waitFor2 message (fun () ->
        elementsWithText cssSelector containingText
            |> List.isEmpty
            |> not
    )
