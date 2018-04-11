module SoManyFeeds.ArticlesData
open System


type Record =
    { Title : string option
    ; Link : string option
    ; Content : string
    ; Date : DateTime option
    ; Source : string
    }


module Repository =
    let private allRecords : Record list =
        [ { Title = (Some "Hello World")
          ; Link = None
          ; Content = "wassup?"
          ; Date = None
          ; Source = "social"
          }
          { Title = None
          ; Link = None
          ; Content = "writing some f#"
          ; Date = (Some DateTime.Now)
          ; Source = "code"
          }
        ]

    let findAll (_: unit) = allRecords
