module SoManyFeedsServer.ErrorPage

module private Views =
    open Fable.React
    open Fable.React.Props

    let error message =
        [ header [ Class "page" ]
              [ div [ Class "page-content" ]
                    [ h2 [] [ str "Error" ]
                      h1 [] [ str "There was a server error" ]
                    ]
              ]
          div [ Class "main" ]
              [ section []
                    [ div [ Class "card" ]
                          [ p [ Class "message" ] [ str $"Oops, there was an error: %s{message}" ]
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
