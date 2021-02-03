open System

#r "../AutoUIShared/bin/Debug/netstandard2.0/AutoUIShared.dll"
#load "Helper.fsx"
open AutoUIShared.AutoUI
open System.Collections.Generic
open Helper


let createInitState () =
    
    let (ui:seq<IAutoUi>) = seq {
       
        yield Button(Id = "BtnReset", Label="Reset App", OnClickEval="Reset")
        yield Label(Text = $"<b>Person App </b>")
        yield Label(Text = $"Output:")
        yield Label( Id = "OutputLabel"  , Text="Init :)") 

        yield Input(Id="NameInput", Label = "A Name:", Value="")
        yield Button(Id = "Btn1", Label="Add Name", OnClickEval="AddName")
        yield Button(Id = "BtnClear", Label="Clear", OnClickEval="Clear")
        yield Label(Text= "<br><br>Result:")
    }
    
    
    
    let resUI = Container(UiElements = List<IAutoUi>(ui))
    resUI        


let execute (base64Json:string) : State=
   
    let bytes =  System.Convert.FromBase64String(base64Json)
    let json = System.Text.Encoding.UTF8.GetString(bytes)
    let currentState = deserializedObject<State>(json)
    let currentUi = currentState.UI
    if (isNull currentUi) then
            currentState.UI <- createInitState()
            currentState
    else 
        let res = currentUi.UiElements.Find(fun ui -> ui.Id = "NameInput")
        let input:Input = downcast res
        
        let outputLabel = currentUi.UiElements.Find(fun ui -> ui.Id = "OutputLabel")
        let outputLabel:Label = downcast outputLabel
        
        // Finished gathering UI elements.
        // Now we can update it :)
        
        input.Label <- "A Name:"
        
        let (message, args) =
            if currentState.Message |> isNull then
                 (null, null)
            else
                 let splitted = currentState.Message.Split(" ")
                 (splitted.[0], splitted.[1..])
             
        match message with
        | "AddName" ->
                if String.IsNullOrWhiteSpace input.Value then
                    outputLabel.Text <- "<span style='color:red'>Please enter a name!</span>"
                    input.Label <- "<span style='color:red'>Enter a name:</span>"
                else
                    let guid = Guid.NewGuid().ToString();
                    currentState.UI.UiElements.Add(Button (
                                                        Label = input.Value,
                                                        Id = "NameBtn"+guid,
                                                        OnClickEval="Remove " + guid
                                                        ))
                    outputLabel.Text <- "Added Name: " + input.Value
        | "Reset" ->
              currentState.UI <- createInitState()
        | "Clear" ->
              let _ = currentState.UI.UiElements.RemoveAll(fun ui -> not (isNull ui.Id) && ui.Id.StartsWith("NameBtn"))
              ()
        | "Remove" ->
               let idToRemove = args.[0]
               let _ = currentState.UI.UiElements.RemoveAll(fun ui -> not (isNull ui.Id) && ui.Id = "NameBtn"+idToRemove )
               ()
              
        | _ -> outputLabel.Text <- $"Could not find handler for message {currentState.Message}"
        
        currentState
