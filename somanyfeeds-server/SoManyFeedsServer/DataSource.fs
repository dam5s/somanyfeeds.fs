module SoManyFeedsServer.DataSource

open System.Data.Common


type DataSource = AsyncResult<DbConnection>


type FindResult<'T> =
    | Found of 'T
    | NotFound
    | FindError of string


type Binding =
    Binding of string * obj


let optionBinding (name : string, option : 'T option) =
    match option with
    | Some value -> Binding (name, value)
    | None -> Binding (name, null)


let inBindings (prefix : string) (values : 'T list) : (string * Binding list) =
    let args =
        values
        |> List.mapi (fun index value -> sprintf "%s%d" prefix index)
        |> String.concat ","

    let bindings =
        values
        |> List.mapi (fun index value -> Binding (sprintf "%s%d" prefix index, value))

    args, bindings


let private fromOptionResult (result : AsyncResult<'T option>) : Async<FindResult<'T>> =
    async {
        match! result with
        | Ok (Some value) ->
            return Found value
        | Ok None ->
            return NotFound
        | Error message ->
            return FindError message
    }


let private usingConnection (dataSource : DataSource) (mapping : DbConnection -> 'T) : AsyncResult<'T> =
   asyncResult {
       let! connection = dataSource

       return! unsafeOperation "Data access" { return fun _ ->
           use c = connection
           c.Open ()
           mapping c
       }
   }


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


let private readFrom (dataSource : DataSource) (sql : string) (bindings : Binding list) (mapping : DbDataReader -> 'T) : AsyncResult<'T> =
    usingConnection dataSource (fun connection ->
        use command = createCommand connection sql bindings
        use reader = command.ExecuteReader ()
        mapping reader
    )


let query dataSource sql (bindings : Binding list) (mapping : DbDataRecord -> 'T) : AsyncResult<'T list> =
    readFrom dataSource sql bindings (fun reader ->
        reader
        |> Seq.cast<DbDataRecord>
        |> Seq.map mapping
        |> Seq.toList
    )


let find dataSource sql (bindings : Binding list) (mapping : DbDataRecord -> 'T) : Async<FindResult<'T>> =
    fromOptionResult <| readFrom dataSource sql bindings (fun reader ->
        reader
        |> Seq.cast<DbDataRecord>
        |> Seq.map mapping
        |> Seq.tryHead
    )


let count dataSource sql (bindings : Binding list) : AsyncResult<int64> =
    readFrom dataSource sql bindings (fun reader ->
        reader
        |> Seq.cast<DbDataRecord>
        |> Seq.map (fun r -> r.GetInt64(0))
        |> Seq.tryHead
        |> Option.defaultValue (int64 0)
    )


let update dataSource sql (bindings : Binding list) : AsyncResult<int> =
    usingConnection dataSource (fun connection ->
        use command = createCommand connection sql bindings
        command.ExecuteNonQuery ()
    )
