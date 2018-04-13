module Server.Feeds

type FeedType =
    | Rss
    | Atom
    | Twitter
    | Custom

type Feed =
    { Name : string
    ; Slug : string
    ; Info : string
    ; Type: FeedType
    }

module Repository =

    let findAll (): Feed list = [
        { Name = "Github"
          Slug = "code"
          Info = "https://github.com/dam5s.atom"
          Type = Atom
        }
        { Name = "Medium"
          Slug = "blog"
          Info = "https://medium.com/feed/@its_damo"
          Type = Rss
        }
        { Name = "Twitter"
          Slug = "social"
          Info = "its_damo"
          Type = Twitter
        }
    ]
