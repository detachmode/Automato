namespace FSI

open System.Diagnostics
open System.Text
open AutoUIShared.AutoUI
open FSharp.Compiler.Interactive.Shell
open System.IO
open FSharp.Compiler.SourceCodeServices
open Newtonsoft.Json

module InteractiveShell =

    // Initialize output and input streams
    let sbOut = new StringBuilder()
    let sbErr = new StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    // Build command line arguments & start FSI session
    let argv = [| "C:\\fsi.exe" |]

    let allArgs =
        Array.append argv [| "--noninteractive" |]

    let fsiConfig =
        FsiEvaluationSession.GetDefaultConfiguration()
    let defaultArgs = [|"fsi.exe";"--noninteractive";"--nologo";"--gui-"|]
    let mutable fsiSession =
        FsiEvaluationSession.Create(fsiConfig, defaultArgs, inStream, outStream, errStream, collectible=true)
//        FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

    let inline toJson obj =
        JsonConvert.SerializeObject(
            obj,
            Formatting.Indented,
            JsonSerializerSettings(
                TypeNameHandling = TypeNameHandling.Objects,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            )
        )
        
    let getErrorOutput (exn:exn) (errorInfos:FSharpErrorInfo[]) =
        let err = errStream.ToString()
        errStream.Flush()
        
        
        let getColor (sev) =
            match sev.ToString().ToLower() with
            | "error" -> "red"
            | _ -> "#ffa500"
            
        let allErros =
            [
                for w in errorInfos do
                    let msg = $"
                        <span style='color:{getColor w.Severity}'> <b>Message:</b>{w.Message}</span><br>
                        <b>Severity:</b>{w.Severity}<br>
                        <b>Line:</b>{w.Start.Line}<br><br>
                        <b>Filename:</b>{w.FileName}<br><br>
                        "
                    yield msg.Trim()
            ]
            
        let formattedWarnings = System.String.Join("<hr>", allErros)
         
        $"
        <h1 class='title'>Could net execute fsx file</h1>
        <b>execution exception:</b>{exn.Message}
        <hr/>
        <hr/>
        <b>Error from stdErr:</b>{err}
        <hr/>
        <b>Errors:</b><br><br>{formattedWarnings}
        <hr/>
        <hr/>"
        
    let loadscript fullpath =
        
//        use session = FsiEvaluationSession.Create(fsiConfig, defaultArgs, inStream, outStream, errStream, collectible=true)


        let content = File.ReadAllText fullpath

        let result, errorInfos =
            fsiSession.EvalInteractionNonThrowing content
            
        // show the result
        let output =
            match result with
            | Choice1Of2 _ -> "checked and executed ok"
            | Choice2Of2 exn -> 
                failwith (getErrorOutput exn errorInfos)

        (output, errorInfos)

    let evalInteraction eval =
        let result, warnings =
            fsiSession.EvalInteractionNonThrowing eval
        // show the result
        let output =
            match result with
            | Choice1Of2 _ -> "checked and executed ok"
            | Choice2Of2 exn -> sprintf "execution exception: %s" exn.Message

        (output, warnings)

    let evalExpression text =
        match fsiSession.EvalExpression(text) with
        | Some value -> printfn "%A" value.ReflectionValue
        | None -> printfn "Got no result!"

    let evalExpressionTyped<'T> (text) =
        let (result, errors) = fsiSession.EvalExpressionNonThrowing(text)

        match result with
        | Choice1Of2 res ->
                match res with
                | Some value -> value.ReflectionValue |> unbox<'T>
                | None -> failwith "Got no result!"
        | Choice2Of2 exn ->
            failwith (getErrorOutput exn errors)


    let app1 (workingDir:string) (ui:Container) : IAutoUi =
        
        let sw = Stopwatch.StartNew ()
        let res = loadscript <| Path.Combine(workingDir, "/app1.fsx")
        
        let mutable base64Json = ""
        if not (isNull ui) then
            let uiJson = toJson ui
            let byt = System.Text.Encoding.UTF8.GetBytes(uiJson)
            base64Json <- System.Convert.ToBase64String(byt)
        
        let loadTime = sw.ElapsedMilliseconds
        let uiFromFsx = evalExpressionTyped<Container>($"execute \"{base64Json}\"")
        let executionTime = sw.ElapsedMilliseconds;
        uiFromFsx.UiElements.Add(Label(Text = $"Until script loaded: {loadTime}; Until executed {executionTime}"))

        
        upcast uiFromFsx
        
        
