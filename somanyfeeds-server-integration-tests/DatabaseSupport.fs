[<AutoOpen>]
module DatabaseSupport

open SoManyFeedsServer


let executeSql (sql : string) =
    DataSource.update DataAccess.dataSource sql [] |> ignore
