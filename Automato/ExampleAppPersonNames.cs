#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using AutoUIShared.AutoUI;

namespace Automato
{
    
    public class ExampleAppPersonNames
    {
        private const string UpdateFun = "Update";
        private const string AddFun = "add";
        private const string RemoveFun = "Remove";

        private IAutoUi GetByID(Container ui, string id)
        {
            
           return ui.UiElements.First(x => x.Id == id);
        }
        
        public IAutoUi Execute(IAutoUi? ui, string action)
        {
            if (ui == null)
            {
                return InitUi();
            }

            var container = ui as Container;
            var cmd = GetAction(action);
            
            switch (cmd)
            {
                case RemoveFun:
                {
                    var split = action.Split(" ");
                    var id = split[1];
                    var btn = GetByID(container, id) as Button;
                    container.UiElements.Remove(btn);
                    return ui;
                }
                   
                case AddFun:
                {
                    var input = GetByID(container, "InputName") as Input;
                    var guid = Guid.NewGuid().ToString();
                    container.UiElements.Add(new Button
                    {
                        Id = guid,
                        Label = input.Value,
                        OnClickEval = "Remove " + guid
                    });
                    return ui;
                }
                case UpdateFun:
                {
                    var btn = GetByID(container, "Btn1") as Button;
                    var input = GetByID(container, "InputName") as Input;
                    btn.Label = input.Value;
                    return ui;
                }
            }

            return ui;

        }

        private static string GetAction(string action)
        {
            var actionSplit = action.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var command = actionSplit.FirstOrDefault();
            return command;
        }

        private static Container InitUi()
        {
            return new()
            {
                UiElements = new List<IAutoUi>
                {
                    new Button
                    {
                        Id = "Btn1",
                        Label = "AutoUIShared Button",
                        OnClickEval = AddFun
                    },
                    new Input
                    {
                        Id = "InputName",
                        Label = "Name",
                        Value = "Nothing entered",
                        OnChangeEval = UpdateFun
                        
                    }
                }
            };
        }
    }
}