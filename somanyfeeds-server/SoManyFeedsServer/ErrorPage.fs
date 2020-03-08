module SoManyFeedsServer.ErrorPage

open Giraffe

module private Views =
    open GiraffeViewEngine

    let error message =
        let fullMessage = sprintf "Oops, there was an error: %s" message

        [ header [ _class "page" ]
              [ div [ _class "page-content" ]
                    [ h2 [] [ rawText "Error" ]
                      h1 [] [ rawText "There was a server error" ]
                    ]
              ]
          div [ _class "main" ]
              [ section []
                    [ div [ _class "card" ]
                          [ p [ _class "message" ] [ encodedText fullMessage ]
                          ]
                    ]
              ]
        ]

type private Logs = Logs
let private logger = Logger<Logs>()

let page (explanation: Explanation) =
    explanation
    |> logger.Error
    |> fun e -> Views.error e.Message
    |> Layout.withoutTabs
    |> htmlView
