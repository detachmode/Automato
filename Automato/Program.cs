using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Automato
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            // var configurationRoot = new ConfigurationRoot(new List<IConfigurationProvider>());
            // var fs = new Fsharp(configurationRoot[]);
            // Stopwatch sw = Stopwatch.StartNew();
            // fs.StartDotnetFsiProcess();

            // var timing = "Results:";
            // await fs.ExecuteInDotnetFsi("12+11");
            // timing += $"1) {(sw.ElapsedMilliseconds + "ms elapsed")}{Environment.NewLine}";
            // await fs.ExecuteInDotnetFsi("42+11");
            // timing += $"2) {(sw.ElapsedMilliseconds + "ms elapsed")}{Environment.NewLine}";
            
            // Console.WriteLine(timing);
            // await Task.Delay(4000);

            


            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}