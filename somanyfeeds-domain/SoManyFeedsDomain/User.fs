module SoManyFeedsDomain.User

type User =
    { Id: int64
      Name: string
    }

let create id name = { Id = id ; Name = name }
