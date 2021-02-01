using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AutoUIShared.AutoUI;
using FSI;
using Microsoft.Extensions.Options;

namespace Automato
{
    public class AutomatoConfig
    {
        public string WorkingDirectory { get; set; }
    }

    public class Fsharp
    {
        private static StreamWriter _streamWriter;
        private static Process _process;
        private static readonly object Lock = new();
        private readonly string _workingDir;

        public Fsharp(IOptionsSnapshot<AutomatoConfig> configuration)
        {
            _workingDir = configuration.Value.WorkingDirectory;
        }

        public IAutoUi App1(Container ui)
        {
            return InteractiveShell.app1(ui);
        }

        public void Foo()
        {
            var _ = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run",
                    WorkingDirectory = _workingDir,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
        }

        public async Task ExecuteInDotnetFsi(string eval)
        {
            StartDotnetFsiProcess();

            if (_streamWriter.BaseStream.CanWrite) await _streamWriter.WriteLineAsync(eval + ";;");
        }

        private void StartDotnetFsiProcess()
        {
            lock (Lock)
            {
                if (_process == null)
                {
                    _process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = "fsi",
                            WorkingDirectory = _workingDir,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        }
                    };
                    _process.OutputDataReceived += StdOut;

                    _process.Start();
                    _process.BeginOutputReadLine();
                    _streamWriter = _process.StandardInput;

                    // p.BeginErrorReadLine();
                }
            }
        }

        private static void StdOut(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}