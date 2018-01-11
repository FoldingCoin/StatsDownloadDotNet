﻿namespace StatsDownload.Core.Tests
{
    using System;

    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class TestFileDownloadProvider
    {
        private DateTime dateTime;

        private IDateTimeService dateTimeServiceMock;

        private IDownloadService downloadServiceMock;

        private IFileDownloadDataStoreService fileDownloadDataStoreServiceMock;

        private IFileDownloadMinimumWaitTimeService fileDownloadMinimumWaitTimeServiceMock;

        private IFilePayloadSettingsService filePayloadSettingsServiceMock;

        private IFilePayloadUploadService filePayloadUploadServiceMock;

        private ILoggingService loggingServiceMock;

        private IResourceCleanupService resourceCleanupServiceMock;

        private IFileDownloadService systemUnderTest;

        [Test]
        public void Constructor_WhenNullDependencyProvided_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                NewFileDownloadProvider(
                    null,
                    loggingServiceMock,
                    downloadServiceMock,
                    filePayloadSettingsServiceMock,
                    resourceCleanupServiceMock,
                    fileDownloadMinimumWaitTimeServiceMock,
                    dateTimeServiceMock,
                    filePayloadUploadServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                NewFileDownloadProvider(
                    fileDownloadDataStoreServiceMock,
                    null,
                    downloadServiceMock,
                    filePayloadSettingsServiceMock,
                    resourceCleanupServiceMock,
                    fileDownloadMinimumWaitTimeServiceMock,
                    dateTimeServiceMock,
                    filePayloadUploadServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                NewFileDownloadProvider(
                    fileDownloadDataStoreServiceMock,
                    loggingServiceMock,
                    null,
                    filePayloadSettingsServiceMock,
                    resourceCleanupServiceMock,
                    fileDownloadMinimumWaitTimeServiceMock,
                    dateTimeServiceMock,
                    filePayloadUploadServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                NewFileDownloadProvider(
                    fileDownloadDataStoreServiceMock,
                    loggingServiceMock,
                    downloadServiceMock,
                    null,
                    resourceCleanupServiceMock,
                    fileDownloadMinimumWaitTimeServiceMock,
                    dateTimeServiceMock,
                    filePayloadUploadServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                NewFileDownloadProvider(
                    fileDownloadDataStoreServiceMock,
                    loggingServiceMock,
                    downloadServiceMock,
                    filePayloadSettingsServiceMock,
                    null,
                    fileDownloadMinimumWaitTimeServiceMock,
                    dateTimeServiceMock,
                    filePayloadUploadServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                NewFileDownloadProvider(
                    fileDownloadDataStoreServiceMock,
                    loggingServiceMock,
                    downloadServiceMock,
                    filePayloadSettingsServiceMock,
                    resourceCleanupServiceMock,
                    null,
                    dateTimeServiceMock,
                    filePayloadUploadServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                NewFileDownloadProvider(
                    fileDownloadDataStoreServiceMock,
                    loggingServiceMock,
                    downloadServiceMock,
                    filePayloadSettingsServiceMock,
                    resourceCleanupServiceMock,
                    fileDownloadMinimumWaitTimeServiceMock,
                    null,
                    filePayloadUploadServiceMock));
        }

        [Test]
        public void DownloadFile_WhenDataStoreIsNotAvailable_ReturnsFailedResultWithReason()
        {
            fileDownloadDataStoreServiceMock.IsAvailable().Returns(false);

            FileDownloadResult actual = InvokeDownloadFile();

            Assert.That(actual.Success, Is.False);
            Assert.That(actual.FailedReason, Is.EqualTo(FailedReason.DataStoreUnavailable));
            Assert.That(actual.FilePayload, Is.InstanceOf<FilePayload>());
        }

        [Test]
        public void DownloadFile_WhenExceptionThrown_ExceptionHandled()
        {
            fileDownloadDataStoreServiceMock.When(mock => mock.IsAvailable()).Do(info => { throw new Exception(); });

            FileDownloadResult actual = InvokeDownloadFile();

            Assert.That(actual.Success, Is.False);
            Assert.That(actual.FailedReason, Is.EqualTo(FailedReason.UnexpectedException));
            Assert.That(actual.FilePayload, Is.InstanceOf<FilePayload>());
        }

        [Test]
        public void DownloadFile_WhenExceptionThrown_LogsException()
        {
            var expected = new Exception();
            fileDownloadDataStoreServiceMock.When(mock => mock.IsAvailable()).Do(info => { throw expected; });

            InvokeDownloadFile();

            Received.InOrder(
                (() =>
                    {
                        loggingServiceMock.LogVerbose("DownloadStatsFile Invoked");
                        fileDownloadDataStoreServiceMock.IsAvailable();
                        loggingServiceMock.LogResult(Arg.Any<FileDownloadResult>());
                        loggingServiceMock.LogException(expected);
                    }));
        }

        [Test]
        public void DownloadFile_WhenExceptionThrown_ResourceCleanupInvoked()
        {
            fileDownloadDataStoreServiceMock.When(mock => mock.IsAvailable()).Do(info => { throw new Exception(); });

            InvokeDownloadFile();

            resourceCleanupServiceMock.Received().Cleanup(Arg.Any<FilePayload>());
        }

        [Test]
        public void DownloadFile_WhenInvoked_ResultIsSuccessAndContainsDownloadData()
        {
            FileDownloadResult actual = InvokeDownloadFile();

            Assert.That(actual.Success, Is.True);
            Assert.That(actual.FilePayload, Is.InstanceOf<FilePayload>());
        }

        [Test]
        public void DownloadFile_WhenInvoked_StatsDownloadIsPerformed()
        {
            InvokeDownloadFile();

            Received.InOrder(
                (() =>
                    {
                        loggingServiceMock.LogVerbose("DownloadStatsFile Invoked");
                        fileDownloadDataStoreServiceMock.IsAvailable();
                        fileDownloadDataStoreServiceMock.UpdateToLatest();
                        dateTimeServiceMock.DateTimeNow();
                        loggingServiceMock.LogVerbose($"Stats file download started: {dateTime}");
                        fileDownloadDataStoreServiceMock.NewFileDownloadStarted(Arg.Any<FilePayload>());
                        fileDownloadMinimumWaitTimeServiceMock.IsMinimumWaitTimeMet(Arg.Any<FilePayload>());
                        filePayloadSettingsServiceMock.SetFilePayloadDownloadDetails(Arg.Any<FilePayload>());
                        downloadServiceMock.DownloadFile(Arg.Any<FilePayload>());
                        dateTimeServiceMock.DateTimeNow();
                        loggingServiceMock.LogVerbose($"Stats file download completed: {dateTime}");
                        filePayloadUploadServiceMock.UploadFile(Arg.Any<FilePayload>());
                        resourceCleanupServiceMock.Cleanup(Arg.Any<FilePayload>());
                        loggingServiceMock.LogResult(Arg.Any<FileDownloadResult>());
                    }));
        }

        [Test]
        public void DownloadFile_WhenMinimumWaitTimeNotMet_LogsDownloadResult()
        {
            fileDownloadMinimumWaitTimeServiceMock.IsMinimumWaitTimeMet(Arg.Any<FilePayload>()).Returns(false);

            InvokeDownloadFile();

            loggingServiceMock.Received().LogResult(Arg.Any<FileDownloadResult>());
        }

        [Test]
        public void DownloadFile_WhenMinimumWaitTimeNotMet_ReturnsFailedResultWithReason()
        {
            fileDownloadMinimumWaitTimeServiceMock.IsMinimumWaitTimeMet(Arg.Any<FilePayload>()).Returns(false);

            FileDownloadResult actual = InvokeDownloadFile();

            Assert.That(actual.Success, Is.False);
            Assert.That(actual.FailedReason, Is.EqualTo(FailedReason.MinimumWaitTimeNotMet));
            Assert.That(actual.FilePayload, Is.InstanceOf<FilePayload>());
        }

        [SetUp]
        public void SetUp()
        {
            dateTime = DateTime.Today;

            fileDownloadDataStoreServiceMock = Substitute.For<IFileDownloadDataStoreService>();
            fileDownloadDataStoreServiceMock.IsAvailable().Returns(true);

            loggingServiceMock = Substitute.For<ILoggingService>();

            downloadServiceMock = Substitute.For<IDownloadService>();

            filePayloadSettingsServiceMock = Substitute.For<IFilePayloadSettingsService>();

            resourceCleanupServiceMock = Substitute.For<IResourceCleanupService>();

            fileDownloadMinimumWaitTimeServiceMock = Substitute.For<IFileDownloadMinimumWaitTimeService>();
            fileDownloadMinimumWaitTimeServiceMock.IsMinimumWaitTimeMet(Arg.Any<FilePayload>()).Returns(true);

            dateTimeServiceMock = Substitute.For<IDateTimeService>();
            dateTimeServiceMock.DateTimeNow().Returns(dateTime);

            filePayloadUploadServiceMock = Substitute.For<IFilePayloadUploadService>();

            systemUnderTest = NewFileDownloadProvider(
                fileDownloadDataStoreServiceMock,
                loggingServiceMock,
                downloadServiceMock,
                filePayloadSettingsServiceMock,
                resourceCleanupServiceMock,
                fileDownloadMinimumWaitTimeServiceMock,
                dateTimeServiceMock,
                filePayloadUploadServiceMock);
        }

        private FileDownloadResult InvokeDownloadFile()
        {
            return systemUnderTest.DownloadStatsFile();
        }

        private IFileDownloadService NewFileDownloadProvider(
            IFileDownloadDataStoreService fileDownloadDataStoreService,
            ILoggingService loggingService,
            IDownloadService downloadService,
            IFilePayloadSettingsService filePayloadSettingsService,
            IResourceCleanupService resourceCleanupService,
            IFileDownloadMinimumWaitTimeService fileDownloadMinimumWaitTimeService,
            IDateTimeService dateTimeService,
            IFilePayloadUploadService filePayloadUploadService)
        {
            return new FileDownloadProvider(
                fileDownloadDataStoreService,
                loggingService,
                downloadService,
                filePayloadSettingsService,
                resourceCleanupService,
                fileDownloadMinimumWaitTimeService,
                dateTimeService,
                filePayloadUploadService);
        }
    }
}