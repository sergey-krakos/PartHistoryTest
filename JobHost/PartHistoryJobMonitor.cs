using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JobHost
{
    internal class PartHistoryJobMonitor
    {
        private const string JobFilesMask = "*.job";

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly IDictionary<Guid, PartHistoryJobInfo> _jobs = new Dictionary<Guid, PartHistoryJobInfo>();

        private readonly string _reportsFolder;
        private readonly PartHistoryJobRepository _jobRepository;

        public PartHistoryJobMonitor(string reportsFolder)
        {
            _reportsFolder = reportsFolder;
            _jobRepository = new PartHistoryJobRepository(_reportsFolder);
        }

        public void Start()
        {
            StartExistingJobs();

            FileSystemWatcher watcher = new FileSystemWatcher(_reportsFolder, JobFilesMask)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };

            watcher.Created += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnChanged;

            watcher.EnableRaisingEvents = true;
        }

        private void StartExistingJobs()
        {
            string[] files = Directory.GetFiles(_reportsFolder, JobFilesMask);
            foreach (string file in files)
            {
                CheckJobFile(file);
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created: // Do the same as Changed.
                case WatcherChangeTypes.Changed:
                    CheckJobFile(e.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    break;
            }
        }

        private void CheckJobFile(string filePath)
        {
            string jobIdStr = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(jobIdStr))
            {
                Trace.TraceError("Cannot get file name from: " + filePath);
                return;
            }

            Guid jobId = Guid.Parse(jobIdStr);

            _lock.EnterReadLock();
            try
            {
                PartHistoryJobInfo job;
                if (_jobs.TryGetValue(jobId, out job))
                {
                    if (job.Status == JobStatus.Aborted)
                    {
                        
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            PartHistoryJobInfo jobInfo = _jobRepository.GetJobInfo(jobId);
            switch (jobInfo.Status)
            {
               case JobStatus.InProgress:
                    StartJob(jobInfo);
                    break;
               case JobStatus.Aborted:
                    break;
            }
        }

        private void StartJob(PartHistoryJobInfo jobInfo)
        {
            jobInfo.TokenSource = new CancellationTokenSource();
            _lock.EnterWriteLock();
            try
            {
                Task t = Task.Factory.StartNew(status => GenerateReport(jobInfo, jobInfo.TokenSource.Token), jobInfo.TokenSource.Token, TaskCreationOptions.LongRunning);
                _jobs.Add(jobInfo.JobId, jobInfo);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void GenerateReport(object jobInfoObj, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            PartHistoryJobInfo jobInfo = (PartHistoryJobInfo)jobInfoObj;
            string reportPath = _jobRepository.GetReportPath(jobInfo.JobId);

            // TODO: ====== Real report generation will be here
            for (int i = 0; i < jobInfo.Seconds; i++)
            {
                using (StreamWriter writer = File.AppendText(reportPath))
                {
                    writer.WriteLine(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                }

                Thread.Sleep(1000);
            }

            _lock.EnterWriteLock();
            try
            {
                jobInfo.Status = JobStatus.Completed;
                _jobRepository.SaveJobInfo(jobInfo);

                _jobs.Remove(jobInfo.JobId);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            // TODO: Notify LTC app
        }
    }
}
