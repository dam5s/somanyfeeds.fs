module Program

open SharpScss
open System
open System.IO


let private generateCss (filePath : string) : string =
    let scssOptions = new ScssOptions (OutputStyle = ScssOutputStyle.Compressed)
    let result = Scss.ConvertFileToCss(filePath, scssOptions)
    result.Css


let private writeToFile (filePath : string) (content : string) =
    Directory.GetParent(filePath).Create()

    use writer = File.CreateText(filePath)
    writer.WriteLine(content)


[<EntryPoint>]
let main (args : string []) =
    let argList = Array.toList args
    let basePath = match argList with
                    | [] -> "."
                    | _ -> argList.Head

    let scssPath = String.Format("{0}/src/scss/app.scss", basePath)
    let cssPath = String.Format("{0}/../server/WebRoot/app.css", basePath)

    generateCss scssPath |> writeToFile cssPath

    printfn "CSS was generated"
    0
