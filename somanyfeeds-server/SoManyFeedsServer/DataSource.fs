module SoManyFeedsServer.DataSource

open System
open System.Data
open System.Data.Common


type DataSource = unit -> Result<DbConnection, string>


type FindResult<'T> =
    | Found of 'T
    | NotFound
    | FindError of string


type Binding =
    Binding of string * obj


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
        printfn "Data access error: %s" (ex.ToString ())
        Error <| String.Format ("Data access error: {0}", ex.Message)


let private applyBinding (command : DbCommand) (Binding (name, value)) =
    let p = command.CreateParameter ()
    p.ParameterName <- name
    p.Value <- value

    command.Parameters.Add p


let private createCommand (connection : DbConnection) (sql : string) (bindings : Binding list) =
    let command = connection.CreateCommand ()
    command.CommandText <- sql
    bindings
        |> List.map (applyBinding command)
        |> ignore
    command


let private readFrom (dataSource : DataSource) (sql : string) (bindings : Binding list) (mapping : DbDataReader -> 'T) : Result<'T, string> =
    usingConnection dataSource (fun connection ->
        use command = createCommand connection sql bindings
        use reader = command.ExecuteReader ()
        mapping reader
    )


let query dataSource sql (bindings : Binding list) (mapping : DbDataRecord -> 'T) : Result<'T list, string> =
    readFrom dataSource sql bindings (fun (reader) ->
        reader
            |> Seq.cast<DbDataRecord>
            |> Seq.map mapping
            |> Seq.toList
    )


let find dataSource sql (bindings : Binding list) (mapping : DbDataRecord -> 'T) : FindResult<'T> =
    fromOptionResult <| readFrom dataSource sql bindings (fun (reader) ->
        reader
            |> Seq.cast<DbDataRecord>
            |> Seq.map mapping
            |> Seq.first
    )


let update dataSource sql (bindings : Binding list) : Result<int, string> =
    usingConnection dataSource (fun connection ->
        use command = createCommand connection sql bindings
        command.ExecuteNonQuery ()
    )
