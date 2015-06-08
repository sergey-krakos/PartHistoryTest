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

        public Guid StartPartHistory(int seconds, string callback)
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

            Guid jobId = StartNewJob(seconds, callback);
            Trace.WriteLine("PartHistoryNew.StartPartHistory. JobId: " + jobId);
            return jobId;
        }

        public AbortPartHistoryOutput AbortPartHistory(Guid jobId)
        {
            Trace.WriteLine("PartHistoryNew.AbortPartHistory - begin.");

            AbortPartHistoryOutput response = new AbortPartHistoryOutput();

            PartHistoryJobInfo jobInfo = _jobRepository.GetJobInfo(jobId);

            response.Status = jobInfo.Status;

            if (jobInfo.Status == JobStatus.Completed)
            {
                Trace.WriteLine("PartHistoryNew.AbortPartHistory - already completed");
            }
            else
            {
                jobInfo.Status = JobStatus.Aborted;
                _jobRepository.SaveJobInfo(jobInfo);
            }

            Trace.WriteLine("PartHistoryNew.AbortPartHistory - end.");
            return response;
        }
        public GetPartHistoryOutput GetPartHistoryStatus(Guid jobId)
        {
            Trace.WriteLine("PartHistoryNew.GetPartHistoryStatus - begin.");

            GetPartHistoryOutput response = new GetPartHistoryOutput();

            PartHistoryJobInfo jobInfo = _jobRepository.GetJobInfo(jobId);

            response.Status = jobInfo.Status;

            if (jobInfo.Status == JobStatus.Completed)
            {
                string fileName = _jobRepository.GetReportPath(jobId);

                Trace.WriteLine("PartHistoryNew.GetPartHistoryStatus - Reading file: " + fileName);
                response.ReturnedDocument = File.ReadAllText(fileName);
                response.IsReady = true;
                response.ErrorCode = ErrorCode.Success;
                
            }
            else
            {
                response.IsReady = false;
            }

            Trace.WriteLine("PartHistoryNew.GetPartHistoryStatus - end.");
            return response;
        }

        private Guid StartNewJob(int seconds, string callback)
        {
            PartHistoryJobInfo jobInfo = new PartHistoryJobInfo(seconds) { CallBackUrl = callback };

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