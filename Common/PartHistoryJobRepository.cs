using System;
using System.IO;
using System.Xml.Linq;

namespace Common
{
    public class PartHistoryJobRepository
    {
        private const string JobExtension = ".job";
        private const string ReportExtension = ".report";

        private readonly string _reportsFolder;

        public PartHistoryJobRepository(string reportsFolder)
        {
            _reportsFolder = reportsFolder;
        }

        public void SaveJobInfo(PartHistoryJobInfo jobInfo)
        {
            string filePath = Path.Combine(_reportsFolder, jobInfo.JobId + JobExtension);
            string fileContent = XmlSerializationUtility.SerializeToXDocument(jobInfo).ToString(SaveOptions.OmitDuplicateNamespaces);
            File.WriteAllText(filePath, fileContent);
        }

        public PartHistoryJobInfo GetJobInfo(Guid jobId)
        {
            string filePath = Path.Combine(_reportsFolder, jobId + JobExtension);
            string fileContent = File.ReadAllText(filePath);
            XDocument document = XDocument.Parse(fileContent);
            PartHistoryJobInfo jobInfo = XmlSerializationUtility.DeserializeFromXDocument<PartHistoryJobInfo>(document);

            return jobInfo;
        }

        public string GetReportPath(Guid jobId)
        {
            string filePath = Path.Combine(_reportsFolder, jobId + ReportExtension);
            return filePath;
        }
    }
}
