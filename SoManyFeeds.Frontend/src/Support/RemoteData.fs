module SoManyFeedsFrontend.Support.RemoteData


type RemoteData<'a> =
    | NotLoaded
    | Loading
    | Loaded of 'a
    | RemoteError of string

[<RequireQualifiedAccess>]
module RemoteData =
    let map f data =
        match data with
        | Loaded d -> Loaded (f d)
        | NotLoaded -> NotLoaded
        | Loading -> Loading
        | RemoteError msg -> RemoteError msg
