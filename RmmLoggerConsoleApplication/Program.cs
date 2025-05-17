using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RmmLoggerConsoleApplication
{
    internal class Program
    {
        private static string _logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + $@"\Temp\RmmLogger.log";
        private static string serviceName = "ProcessLoggerService";
        static void Main(string[] args)
        {
            Console.WriteLine("Tactical RMM Logger Extension - By Vichingo455");
            foreach (string arg in args)
            {
                if (arg == "--getlogs")
                {
                    try
                    {
                        Console.WriteLine("Logs:");
                        Console.Write(File.ReadAllText(_logFilePath));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    break;
                }
                else if (arg == "--servicestatus")
                {
                    try
                    {
                        ServiceController serviceController = new ServiceController(serviceName);
                        ServiceControllerStatus status = serviceController.Status;
                        Console.WriteLine($"Service '{serviceName}' is currently: {status}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    break;
                }
                else if (arg == "--flushlogs")
                {
                    try
                    {
                        File.Delete(_logFilePath);
                        Console.WriteLine("Log file flushed successfully!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    break;
                }
            }
            Console.WriteLine();
        }
    }
}
