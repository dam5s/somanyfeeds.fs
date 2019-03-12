[<AutoOpen>]
module DatabaseSupport

open SoManyFeedsServer


let executeSql (sql : string) =
    DataSource.update DataSource.dataSource sql []
    |> Async.RunSynchronously
    |> ignore
