﻿namespace StatsDownload.Core
{
    using System.IO;

    public class FileReaderProvider : IFileReaderService
    {
        public void ReadFile(StatsPayload statsPayload)
        {
            using (var reader = new StreamReader(statsPayload.DownloadFileName))
            {
                statsPayload.StatsData = reader.ReadToEnd();
            }
        }
    }
}