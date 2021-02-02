using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Automato;
using AutoUIShared.AutoUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AutomatoCore
{

    public static class Extensions
    {
        public static void AddAutomato(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<AutoEngine>();
            services.AddScoped<Fsharp>();
            services.Configure<AutomatoConfig>(config.GetSection("Automato"));
        }
    }
    public class AutoEngine
    {
        private readonly IOptionsSnapshot<AutomatoConfig> _config;
        private readonly Fsharp _fsharp;

        private bool lastExecutionFailed;

        public AutoEngine(Fsharp fsharp, IOptionsSnapshot<AutomatoConfig> config)
        {
            _fsharp = fsharp;
            _config = config;
        }

        public State State { get; set; } = new();

        public string Output { get; set; }

        public event Func<Task> OnStateHasChanged;


        public void ClickButton(Button btn)
        {
            Output = btn.OnClickEval;
            State.Message = btn.OnClickEval;
            ExecuteAndCatchErrors();
        }
        
        public void UpdateInput(object value, Input sender)
        {
            Console.WriteLine($"sender {sender.Label} value changed to {value}");
            sender.Value =value.ToString();
            // if (!string.IsNullOrWhiteSpace(sender.OnChangeEval))
            // {
            //     App.Execute(UI, sender.OnChangeEval);
            // }
        }


        public async void LoadApp1()
        {
            var lastEval = DateTime.MinValue;
            while (true)
            {
                var lastWrite = File.GetLastWriteTimeUtc(Path.Combine(_config.Value.WorkingDirectory, "App1.fsx"));
                if (lastWrite != lastEval)
                {
                    Output = "Loading App1";
                    StateHasChanged();

                    await Task.Delay(10);
                    ExecuteAndCatchErrors();
                    Output = "loaded!";
                    lastEval = lastWrite;

                    StateHasChanged();
                }

                await Task.Delay(100);
            }
        }


        private void ExecuteAndCatchErrors()
        {
            try
            {
                State = _fsharp.App1(lastExecutionFailed ? new State() : State);
                lastExecutionFailed = false;
            }
            catch (Exception e)
            {
                lastExecutionFailed = true;
                State.UI = new Container
                {
                    UiElements = new List<IAutoUi>
                    {
                        new Label
                        {
                            Text = "<b>Failed to execute App1 script:" +
                                   "</b><br/>" +
                                   $"{e.Message}" +
                                   "<hr><hr>" +
                                   "<b>CompleteCaught Exception in Blazor</b>" +
                                   $"{e}"
                        }
                    }
                };
            }
        }

        private void StateHasChanged()
        {
            OnStateHasChanged?.Invoke();
        }
    }
}