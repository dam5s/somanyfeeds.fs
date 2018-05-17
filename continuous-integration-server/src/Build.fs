module Build

open Git


type BuildLog =
    | BuildLog of string


type BuildStatus =
    | Pending
    | Running
    | Successful of BuildLog
    | Failed of BuildLog


type Build =
    { id : int
    ; sha : Sha
    ; status : BuildStatus
    }

type DockerScript =
    { image : string
    ; scriptName : string
    }
