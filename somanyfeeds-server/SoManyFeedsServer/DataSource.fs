module SoManyFeedsServer.DataSource

open System
open System.Data
open System.Data.Common


type DataSource = unit -> Result<DbConnection, string>


type FindResult<'T> =
    | Found of 'T
    | NotFound
    | FindError of string


let private fromOptionResult (result : Result<'T option, string>) : FindResult<'T> =
    match result with
    | Ok (Some value) -> Found value
    | Ok None -> NotFound
    | Error message -> FindError message


let private usingConnection (dataSource : DataSource) (mapping : DbConnection -> 'T) : Result<'T, string> =
    try
        dataSource ()
            |> Result.map (fun (c : DbConnection) ->
                use connection = c
                connection.Open ()
                mapping connection
            )
    with
    | ex ->
        Error <| String.Format ("Data access error: {0}", ex.Message)


let private readFrom (dataSource : DataSource) (sql : string) (bindings : DbParameterCollection -> DbParameterCollection) (mapping : DbDataReader -> 'T) : Result<'T, string> =
    usingConnection dataSource (fun connection ->
        use command = connection.CreateCommand ()
        command.CommandText <- sql
        bindings command.Parameters |> ignore

        use reader = command.ExecuteReader ()
        mapping reader
    )


let param (name : string) (value : obj) (parameters : DbParameterCollection) : DbParameterCollection  =
    parameters.Add (name, value) |> ignore
    parameters


let noParams = id


let query dataSource sql (bindings : DbParameterCollection -> DbParameterCollection) (mapping : DbDataRecord -> 'T) : Result<'T list, string> =
    readFrom dataSource sql bindings (fun (reader) ->
        reader
            |> Seq.cast<DbDataRecord>
            |> Seq.map mapping
            |> Seq.toList
    )


let find dataSource sql (bindings : DbParameterCollection -> DbParameterCollection) (mapping : DbDataRecord -> 'T) : FindResult<'T> =
    fromOptionResult <| readFrom dataSource sql bindings (fun (reader) ->
        reader
            |> Seq.cast<DbDataRecord>
            |> Seq.map mapping
            |> Seq.first
    )


let update dataSource sql (bindings : DbParameterCollection -> DbParameterCollection) : Result<int, string> =
    usingConnection dataSource (fun connection ->
        use command = connection.CreateCommand ()
        command.CommandText <- sql
        bindings command.Parameters |> ignore

        command.ExecuteNonQuery ()
    )
