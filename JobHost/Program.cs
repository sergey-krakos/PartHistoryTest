using System;
using System.Diagnostics;
using System.IO;

namespace JobHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string reportsFolder = args[0];

            InitApp(reportsFolder);

            PartHistoryJobMonitor jobMonitor = new PartHistoryJobMonitor(reportsFolder);
            jobMonitor.Start();

            Console.Out.WriteLine("Started.");
            Console.In.Read();
        }

        private static void InitApp(string reportsFolder)
        {
            string logsFile = Path.Combine(reportsFolder, "serviceProc.log");
            Trace.Listeners.Add(new TextWriterTraceListener(logsFile));
            Trace.AutoFlush = true;

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Trace.WriteLine("Process Started");
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(e.ExceptionObject);
        }
    }
}
