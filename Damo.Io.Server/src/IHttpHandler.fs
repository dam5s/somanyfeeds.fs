module Damo.Io.Server.IHttpHandler

open Giraffe
open Microsoft.AspNetCore.Http

type IHttpHandler =
    abstract member Handle: HttpFunc * HttpContext -> HttpFuncResult

let handler<'a when 'a :> IHttpHandler> : HttpHandler =
    fun next ctx -> ctx.GetService<'a>().Handle(next, ctx)
