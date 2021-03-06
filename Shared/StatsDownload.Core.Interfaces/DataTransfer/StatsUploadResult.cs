﻿namespace StatsDownload.Core.Interfaces.DataTransfer
{
    using StatsDownload.Core.Interfaces.Enums;

    public class StatsUploadResult
    {
        public StatsUploadResult()
            : this(true, 0, FailedReason.None)
        {
        }

        public StatsUploadResult(int downloadId, FailedReason failedReason)
            : this(false, downloadId, failedReason)
        {
        }

        private StatsUploadResult(bool success, int downloadId, FailedReason failedReason)
        {
            Success = success;
            DownloadId = downloadId;
            FailedReason = failedReason;
        }

        public int DownloadId { get; }

        public FailedReason FailedReason { get; }

        public bool Success { get; }
    }
}