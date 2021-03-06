﻿namespace StatsDownload.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using StatsDownload.Core.Interfaces;
    using StatsDownload.Core.Interfaces.DataTransfer;
    using StatsDownload.Core.Interfaces.Enums;

    public class ErrorMessageProvider : IErrorMessageService
    {
        public string GetErrorMessage(FailedReason failedReason, FilePayload filePayload, StatsDownloadService service)
        {
            if (failedReason == FailedReason.MinimumWaitTimeNotMet)
            {
                TimeSpan minimumWaitTimeSpan = MinimumWait.TimeSpan;
                TimeSpan configuredWaitTime = filePayload.MinimumWaitTimeSpan;
                return string.Format(ErrorMessages.MinimumWaitTimeNotMet, minimumWaitTimeSpan, configuredWaitTime);
            }

            return GetErrorMessage(failedReason, service);
        }

        public string GetErrorMessage(FailedReason failedReason, StatsDownloadService service)
        {
            if (failedReason == FailedReason.DatabaseUnavailable)
            {
                return GetErrorMessageByServiceType(service, ErrorMessages.FileDownloadDatabaseUnavailable,
                    ErrorMessages.StatsUploadDatabaseUnavailable, ErrorMessages.DefaultDatabaseUnavailable);
            }

            if (failedReason == FailedReason.DataStoreUnavailable)
            {
                return ErrorMessages.DataStoreUnavailable;
            }

            if (failedReason == FailedReason.DatabaseMissingRequiredObjects)
            {
                return GetErrorMessageByServiceType(service, ErrorMessages.FileDownloadMissingRequiredObjects,
                    ErrorMessages.StatsUploadMissingRequiredObjects, ErrorMessages.DefaultMissingRequiredObjects);
            }

            if (failedReason == FailedReason.RequiredSettingsInvalid)
            {
                return ErrorMessages.RequiredSettingsAreInvalid;
            }

            if (failedReason == FailedReason.FileDownloadTimeout)
            {
                return ErrorMessages.FileDownloadTimedOut;
            }

            if (failedReason == FailedReason.FileDownloadNotFound)
            {
                return ErrorMessages.FileDownloadNotFound;
            }

            if (failedReason == FailedReason.FileDownloadFailedDecompression)
            {
                return ErrorMessages.FileDownloadFailedDecompression;
            }

            if (failedReason == FailedReason.InvalidStatsFileUpload)
            {
                return ErrorMessages.InvalidStatsFile;
            }

            if (failedReason == FailedReason.UnexpectedDatabaseException)
            {
                return ErrorMessages.StatsUploadTimeout;
            }

            if (failedReason == FailedReason.UnexpectedException)
            {
                return GetErrorMessageByServiceType(service, ErrorMessages.FileDownloadUnexpectedException,
                    ErrorMessages.StatsUploadUnexpectedException, ErrorMessages.DefaultUnexpectedException);
            }

            if (failedReason == FailedReason.UnexpectedValidationException)
            {
                return ErrorMessages.UnexpectedValidationException;
            }

            return string.Empty;
        }

        public string GetErrorMessage(IEnumerable<FailedUserData> failedUsersData)
        {
            string failedLinesMessage = string.Format(ErrorMessages.FailedUserDataCount, failedUsersData.Count());
            string topTenFailedUsers = GetTopFailedUserMessages(failedUsersData, 10);

            return $"{failedLinesMessage}{Environment.NewLine}{Environment.NewLine}{topTenFailedUsers}";
        }

        public string GetErrorMessage(FailedUserData failedUserData)
        {
            RejectionReason rejectionReason = failedUserData.RejectionReason;
            string data = failedUserData.Data;

            if (rejectionReason == RejectionReason.UnexpectedFormat)
            {
                return string.Format(ErrorMessages.UserDataUnexpectedFormat, data);
            }

            if (rejectionReason == RejectionReason.FailedParsing)
            {
                return string.Format(ErrorMessages.FailedParsingUserData, data);
            }

            string name = failedUserData.UserData?.Name?.Length > 175 ? failedUserData.UserData?.Name?.Substring(0, 175)
                              : failedUserData.UserData?.Name;

            if (rejectionReason == RejectionReason.FahNameExceedsMaxSize)
            {
                return FormatExceedsMaxSizeRejection(ErrorMessages.FahNameExceedsMaxSize, name,
                    failedUserData.UserData?.Name?.Length ?? 0);
            }

            if (rejectionReason == RejectionReason.FriendlyNameExceedsMaxSize)
            {
                return FormatExceedsMaxSizeRejection(ErrorMessages.FriendlyNameExceedsMaxSize, name,
                    failedUserData.UserData?.FriendlyName?.Length ?? 0);
            }

            if (rejectionReason == RejectionReason.BitcoinAddressExceedsMaxSize)
            {
                return FormatExceedsMaxSizeRejection(ErrorMessages.BitcoinAddressExceedsMaxSize, name,
                    failedUserData.UserData?.BitcoinAddress?.Length ?? 0);
            }

            if (rejectionReason == RejectionReason.FailedAddToDatabase)
            {
                return string.Format(ErrorMessages.FailedAddUserToDatabase, name);
            }

            return string.Empty;
        }

        private string FormatExceedsMaxSizeRejection(string format, string data, int actualSize)
        {
            return string.Format(format, data, actualSize);
        }

        private string GetErrorMessageByServiceType(StatsDownloadService service, string fileDownloadMessage,
                                                    string statsUploadMessage, string defaultMessage)
        {
            if (service == StatsDownloadService.FileDownload)
            {
                return fileDownloadMessage;
            }

            if (service == StatsDownloadService.StatsUpload)
            {
                return statsUploadMessage;
            }

            return defaultMessage;
        }

        private string GetTopFailedUserMessages(IEnumerable<FailedUserData> failedUsersData, int numberOfUsers)
        {
            int failedUsersCount = failedUsersData.Count();

            string message = $"Top {numberOfUsers} Failed Users:";

            for (var userIndex = 0; userIndex < numberOfUsers && userIndex < failedUsersCount; userIndex++)
            {
                message += $"{Environment.NewLine}{GetErrorMessage(failedUsersData.ElementAt(userIndex))}";
            }

            return message;
        }
    }
}