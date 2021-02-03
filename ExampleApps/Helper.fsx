open System.Diagnostics
open System

#r "../Automato/bin/Debug/net5.0/Newtonsoft.Json.dll"
open Newtonsoft.Json


let inline toJson obj =
    JsonConvert.SerializeObject(
        obj,
        Formatting.Indented,
        JsonSerializerSettings(
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
        )
    )

let deserializedObject<'T> json : 'T = 
    JsonConvert.DeserializeObject<'T>(json, JsonSerializerSettings(TypeNameHandling = TypeNameHandling.Objects))


let runProc filename args startDir =
    printf "Run"
    let timer = Stopwatch.StartNew()

    let procStartInfo =
        ProcessStartInfo(
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = filename,
            Arguments = args
        )

    match startDir with
    | Some d -> procStartInfo.WorkingDirectory <- d
    | _ -> ()

    let outputs =
        System.Collections.Generic.List<string>()

    let errors =
        System.Collections.Generic.List<string>()

    let outputHandler f (_sender: obj) (args: DataReceivedEventArgs) = f args.Data
    let p = new Process(StartInfo = procStartInfo)
    p.OutputDataReceived.AddHandler(DataReceivedEventHandler(outputHandler outputs.Add))
    p.ErrorDataReceived.AddHandler(DataReceivedEventHandler(outputHandler errors.Add))

    let started =
        try
            p.Start()
        with ex ->
            ex.Data.Add("filename", filename)
            reraise ()

    if not started then
        failwithf "Failed to start process %s" filename

    printfn "Started %s with pid %i" p.ProcessName p.Id
    p.BeginOutputReadLine()
    p.BeginErrorReadLine()
    p.WaitForExit()
    timer.Stop()
    printfn "Finished %s after %A milliseconds" filename timer.ElapsedMilliseconds

    let cleanOut l =
        l
        |> Seq.filter (fun o -> String.IsNullOrEmpty o |> not)

    cleanOut outputs, cleanOut errors


