module SoManyFeedsFrontend.Support.Form

open SoManyFeedsFrontend.Support.Http

type Form<'Model, 'ValidationError> =
    private { ServerError: string option
              FieldErrors: FieldError<'ValidationError> list
              Model: 'Model
              ErrorToString: 'ValidationError -> string }

[<RequireQualifiedAccess>]
module Form =
    let private removeServerError form =
        { form with ServerError = None }
    let private removeFieldErrors name form =
        { form with FieldErrors = form.FieldErrors |> List.filter (fun e -> e.FieldName <> name) }
    let private addFieldErrors fieldErrors form =
        { form with FieldErrors = form.FieldErrors |> List.append fieldErrors }
    let private addErrorsIfValidationFailed validation form =
        match validation with
        | Ok _ -> form
        | Error fieldErrors -> addFieldErrors fieldErrors form

    let create initialModel errorToString =
        { ServerError = None
          FieldErrors = []
          Model = initialModel
          ErrorToString = errorToString }

    let serverError form = form.ServerError

    let fieldError name form =
        form.FieldErrors
        |> List.tryFind (fun f -> f.FieldName = name)
        |> Option.map (fun f -> form.ErrorToString f.Error)
        |> Option.defaultValue ""

    let model form = form.Model

    let update updater form = { form with Model = updater form.Model }

    let validateField (fieldName: string) validation form =
        form
        |> removeFieldErrors fieldName
        |> addErrorsIfValidationFailed (validation form.Model)

    let isValid form =
        Option.isSome form.ServerError || not (List.isEmpty form.FieldErrors)

    let applyValidationErrors form errors =
        form
        |> removeServerError
        |> addFieldErrors errors

    let applyRequestError (err: RequestError) form =
        { form with ServerError = Some (RequestError.userMessage err) }
