using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using Common;

namespace WebServices
{
    // NOTE: In order to launch WCF Test Client for testing this service, please select PartHistoryNew.svc or PartHistoryNew.svc.cs at the Solution Explorer and start debugging.
    public class PartHistoryNew : IPartHistoryNew
    {
        private readonly string _reportsFolder = ConfigurationManager.AppSettings["ReportsFolder"];
        private readonly PartHistoryJobRepository _jobRepository;

        public PartHistoryNew()
        {
            _jobRepository = new PartHistoryJobRepository(_reportsFolder);
        }

        public Guid StartPartHistory(int seconds)
        {
            string jobHostAppPath = ConfigurationManager.AppSettings["JobHostApp"];
            string processName = Path.GetFileNameWithoutExtension(jobHostAppPath);

            bool isAlreadyStarted;
            using (Mutex instanceMutex = new Mutex(false, processName, out isAlreadyStarted))
            {
                if (!isAlreadyStarted && instanceMutex.WaitOne(0, false))
                {
                    StartJobHostProcess();
                    Trace.WriteLine("PartHistoryNew.StartPartHistory - JobHost started.");
                }
            }

            Guid jobId = StartNewJob(seconds);
            Trace.WriteLine("PartHistoryNew.StartPartHistory. JobId: " + jobId);
            return jobId;
        }

        public GetPartHistoryOutput GetPartHistoryStatus(Guid jobId)
        {
            Trace.WriteLine("PartHistoryNew.GetPartHistoryStatus - begin.");

            GetPartHistoryOutput response = new GetPartHistoryOutput();

            PartHistoryJobInfo jobInfo = _jobRepository.GetJobInfo(jobId);
            if (jobInfo.Status == JobStatus.Completed)
            {
                string fileName = _jobRepository.GetReportPath(jobId);

                Trace.WriteLine("PartHistoryNew.GetPartHistoryStatus - Reading file: " + fileName);
                response.ReturnedDocument = File.ReadAllText(fileName);
                response.IsReady = true;
            }
            else
            {
                response.IsReady = false;
            }

            Trace.WriteLine("PartHistoryNew.GetPartHistoryStatus - end.");
            return response;
        }

        private Guid StartNewJob(int seconds)
        {
            PartHistoryJobInfo jobInfo = new PartHistoryJobInfo(seconds);
            _jobRepository.SaveJobInfo(jobInfo);

            return jobInfo.JobId;
        }

        private void StartJobHostProcess()
        {
            string appPath = ConfigurationManager.AppSettings["JobHostApp"];

            ProcessStartInfo info = new ProcessStartInfo(appPath)
            {
                Arguments = _reportsFolder,
                WorkingDirectory = Path.GetDirectoryName(appPath) ?? ".",
                UseShellExecute = false
            };

            using (HostingEnvironment.Impersonate())
            {
                Trace.WriteLine("PartHistoryNew.StartJobHostProcess - Starting process.");
                Process.Start(info);
                Trace.WriteLine("PartHistoryNew.StartJobHostProcess - Process started.");
            }
        }
    }
}
