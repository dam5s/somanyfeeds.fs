module BuildRepository

open Git
open Build

let mutable private allBuilds : Build list = []


let findAll () : Build list = allBuilds

let findLast10 () : Build List =
    let howMany = min 10 (List.length allBuilds)

    allBuilds |> List.take howMany

let latest () : Build option = List.tryHead allBuilds


let upsert (build : Build) : Build =
    let shouldUpdate = List.exists (fun b -> b.id = build.id) allBuilds
    let doUpsert =
        if shouldUpdate then
            List.map (fun b -> if b.id = build.id then build else b)
        else
            List.append [ build ]

    allBuilds <- doUpsert allBuilds

    build


let create (sha : Sha) : Build =
    upsert <| { id = (List.length allBuilds) + 1
                sha = sha
                status = Pending
              }
