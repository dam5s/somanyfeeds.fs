module DatabaseSupport

open SoManyFeedsServer.DataSource
open SoManyFeedsServer.App


let private dataSource = DataAccess.dataSource


let executeSql (sql : string) =
    update dataSource sql [] |> ignore
