﻿namespace StatsDownload.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    using StatsDownload.Core.Interfaces;
    using StatsDownload.Core.Interfaces.DataTransfer;
    using StatsDownload.Core.Interfaces.Enums;
    using StatsDownload.Core.Interfaces.Logging;

    public class FileDownloadDatabaseProvider : IFileDownloadDatabaseService
    {
        private readonly ILoggingService loggingService;

        private readonly IStatsDownloadDatabaseParameterService statsDownloadDatabaseParameterService;

        private readonly IStatsDownloadDatabaseService statsDownloadDatabaseService;

        public FileDownloadDatabaseProvider(IStatsDownloadDatabaseService statsDownloadDatabaseService,
                                            IStatsDownloadDatabaseParameterService
                                                statsDownloadDatabaseParameterService, ILoggingService loggingService)
        {
            this.statsDownloadDatabaseService = statsDownloadDatabaseService
                                                ?? throw new ArgumentNullException(
                                                    nameof(statsDownloadDatabaseService));
            this.statsDownloadDatabaseParameterService = statsDownloadDatabaseParameterService
                                                         ?? throw new ArgumentNullException(
                                                             nameof(statsDownloadDatabaseParameterService));
            this.loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public void FileDownloadError(FileDownloadResult fileDownloadResult)
        {
            loggingService.LogMethodInvoked();
            CreateDatabaseConnectionAndExecuteAction(service => { FileDownloadError(service, fileDownloadResult); });
        }

        public void FileDownloadFinished(FilePayload filePayload)
        {
            loggingService.LogMethodInvoked();
            CreateDatabaseConnectionAndExecuteAction(service => { FileDownloadFinished(service, filePayload); });
        }

        public void FileDownloadStarted(FilePayload filePayload)
        {
            loggingService.LogMethodInvoked();
            int downloadId = default;
            CreateDatabaseConnectionAndExecuteAction(service => { downloadId = FileDownloadStarted(service); });
            filePayload.DownloadId = downloadId;
        }

        public void FileValidated(FilePayload filePayload)
        {
            loggingService.LogMethodInvoked();
            CreateDatabaseConnectionAndExecuteAction(service => { FileValidated(service, filePayload); });
        }

        public void FileValidationError(FileDownloadResult fileDownloadResult)
        {
            loggingService.LogMethodInvoked();
            CreateDatabaseConnectionAndExecuteAction(service => { FileValidationError(service, fileDownloadResult); });
        }

        public void FileValidationStarted(FilePayload filePayload)
        {
            loggingService.LogMethodInvoked();
            CreateDatabaseConnectionAndExecuteAction(service => { FileValidationStarted(service, filePayload); });
        }

        public DateTime GetLastFileDownloadDateTime()
        {
            loggingService.LogMethodInvoked();
            DateTime lastFileDownloadDateTime = default;
            CreateDatabaseConnectionAndExecuteAction(service =>
            {
                lastFileDownloadDateTime = GetLastFileDownloadDateTime(service);
            });
            return lastFileDownloadDateTime;
        }

        public (bool isAvailable, FailedReason reason) IsAvailable()
        {
            (bool isAvailable, DatabaseFailedReason reason) =
                statsDownloadDatabaseService.IsAvailable(Constants.FileDownloadDatabase.FileDownloadObjects);

            FailedReason failedReason;

            if (reason == DatabaseFailedReason.None)
            {
                failedReason = FailedReason.None;
            }
            else if (reason == DatabaseFailedReason.DatabaseUnavailable)
            {
                failedReason = FailedReason.DatabaseUnavailable;
            }
            else if (reason == DatabaseFailedReason.DatabaseMissingRequiredObjects)
            {
                failedReason = FailedReason.DatabaseMissingRequiredObjects;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            return (isAvailable, failedReason);
        }

        public void UpdateToLatest()
        {
            loggingService.LogMethodInvoked();
            CreateDatabaseConnectionAndExecuteAction(UpdateToLatest);
        }

        private void CreateDatabaseConnectionAndExecuteAction(Action<IDatabaseConnectionService> action)
        {
            statsDownloadDatabaseService.CreateDatabaseConnectionAndExecuteAction(action);
        }

        private DbParameter CreateDownloadIdParameter(IDatabaseConnectionService databaseConnection, int downloadId)
        {
            return statsDownloadDatabaseParameterService.CreateDownloadIdParameter(databaseConnection, downloadId);
        }

        private void FileDownloadError(IDatabaseConnectionService databaseConnection,
                                       FileDownloadResult fileDownloadResult)
        {
            FilePayload filePayload = fileDownloadResult.FilePayload;

            DbParameter downloadId = CreateDownloadIdParameter(databaseConnection, filePayload.DownloadId);

            DbParameter errorMessage =
                statsDownloadDatabaseParameterService.CreateErrorMessageParameter(databaseConnection,
                    fileDownloadResult);

            databaseConnection.ExecuteStoredProcedure(Constants.FileDownloadDatabase.FileDownloadErrorProcedureName,
                new List<DbParameter> { downloadId, errorMessage });
        }

        private void FileDownloadFinished(IDatabaseConnectionService databaseConnection, FilePayload filePayload)
        {
            DbParameter downloadId = CreateDownloadIdParameter(databaseConnection, filePayload.DownloadId);

            DbParameter filePath = databaseConnection.CreateParameter("@FilePath", DbType.String,
                ParameterDirection.Input);
            filePath.Value = filePayload.UploadPath;

            DbParameter fileName = databaseConnection.CreateParameter("@FileName", DbType.String,
                ParameterDirection.Input);
            fileName.Value = filePayload.DownloadFileName;

            DbParameter fileExtension = databaseConnection.CreateParameter("@FileExtension", DbType.String,
                ParameterDirection.Input);
            fileExtension.Value = filePayload.DownloadFileExtension;

            databaseConnection.ExecuteStoredProcedure(Constants.FileDownloadDatabase.FileDownloadFinishedProcedureName,
                new List<DbParameter> { downloadId, filePath, fileName, fileExtension });
        }

        private int FileDownloadStarted(IDatabaseConnectionService databaseConnection)
        {
            DbParameter downloadId =
                statsDownloadDatabaseParameterService.CreateDownloadIdParameter(databaseConnection,
                    ParameterDirection.Output);

            databaseConnection.ExecuteStoredProcedure(Constants.FileDownloadDatabase.FileDownloadStartedProcedureName,
                new List<DbParameter> { downloadId });

            return (int)downloadId.Value;
        }

        private void FileValidated(IDatabaseConnectionService databaseConnection, FilePayload filePayload)
        {
            DbParameter downloadId = CreateDownloadIdParameter(databaseConnection, filePayload.DownloadId);

            DbParameter fileUtcDateTime =
                databaseConnection.CreateParameter("@FileUtcDateTime", DbType.DateTime, ParameterDirection.Input);
            fileUtcDateTime.Value = filePayload.FileUtcDateTime;

            DbParameter filePath = databaseConnection.CreateParameter("@FilePath", DbType.String,
                ParameterDirection.Input);
            filePath.Value = filePayload.UploadPath;

            DbParameter fileName = databaseConnection.CreateParameter("@FileName", DbType.String,
                ParameterDirection.Input);
            fileName.Value = filePayload.DownloadFileName;

            DbParameter fileExtension = databaseConnection.CreateParameter("@FileExtension", DbType.String,
                ParameterDirection.Input);
            fileExtension.Value = filePayload.DownloadFileExtension;

            databaseConnection.ExecuteStoredProcedure(Constants.FileDownloadDatabase.FileValidatedProcedureName,
                new List<DbParameter>
                {
                    downloadId,
                    fileUtcDateTime,
                    filePath,
                    fileName,
                    fileExtension
                });
        }

        private void FileValidationError(IDatabaseConnectionService databaseConnection,
                                         FileDownloadResult fileDownloadResult)
        {
            FilePayload filePayload = fileDownloadResult.FilePayload;

            DbParameter downloadId = CreateDownloadIdParameter(databaseConnection, filePayload.DownloadId);

            DbParameter errorMessage =
                statsDownloadDatabaseParameterService.CreateErrorMessageParameter(databaseConnection,
                    fileDownloadResult);

            databaseConnection.ExecuteStoredProcedure(Constants.FileDownloadDatabase.FileValidationErrorProcedureName,
                new List<DbParameter> { downloadId, errorMessage });
        }

        private void FileValidationStarted(IDatabaseConnectionService databaseConnection, FilePayload filePayload)
        {
            DbParameter downloadId = CreateDownloadIdParameter(databaseConnection, filePayload.DownloadId);

            databaseConnection.ExecuteStoredProcedure(Constants.FileDownloadDatabase.FileValidationStartedProcedureName,
                new List<DbParameter> { downloadId });
        }

        private DateTime GetLastFileDownloadDateTime(IDatabaseConnectionService databaseConnection)
        {
            return databaseConnection.ExecuteScalar(Constants.FileDownloadDatabase.GetLastFileDownloadDateTimeSql) as
                       DateTime? ?? default;
        }

        private void LogVerbose(string message)
        {
            loggingService.LogVerbose(message);
        }

        private void UpdateToLatest(IDatabaseConnectionService databaseConnection)
        {
            int numberOfRowsEffected =
                databaseConnection.ExecuteStoredProcedure(Constants
                                                          .FileDownloadDatabase.UpdateToLatestStoredProcedureName);

            LogVerbose($"'{numberOfRowsEffected}' rows were effected");
        }
    }
}