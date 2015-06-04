using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace JobHost
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isAlreadyStarted;

            string jobHostAppPath = Assembly.GetExecutingAssembly().Location;
            string processName = Path.GetFileNameWithoutExtension(jobHostAppPath);

            using (Mutex instanceMutex = new Mutex(false, processName, out isAlreadyStarted))
            {
                if (isAlreadyStarted || instanceMutex.WaitOne(100, false))
                {
                    Console.Out.WriteLine("Already started.");
                    Console.In.Read();
                    return;
                }

                string reportsFolder = args[0];

                InitApp(reportsFolder);

                PartHistoryJobMonitor jobMonitor = new PartHistoryJobMonitor(reportsFolder);
                jobMonitor.Start();

                Console.Out.WriteLine("Started.");
                Console.In.Read();

                instanceMutex.ReleaseMutex();
            }
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
