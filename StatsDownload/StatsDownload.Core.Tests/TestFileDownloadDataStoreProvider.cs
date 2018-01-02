﻿namespace StatsDownload.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;

    using NSubstitute;
    using NSubstitute.ClearExtensions;

    using NUnit.Framework;

    [TestFixture]
    public class TestFileDownloadDataStoreProvider
    {
        private const int NumberOfRowsEffectedExpected = 5;

        private IDatabaseConnectionServiceFactory databaseConnectionServiceFactoryMock;

        private IDatabaseConnectionService databaseConnectionServiceMock;

        private IDatabaseConnectionSettingsService databaseConnectionSettingsServiceMock;

        private FilePayload filePayload;

        private ILoggingService loggingServiceMock;

        private IFileDownloadDataStoreService systemUnderTest;

        [Test]
        public void Constructor_WhenNullDependencyProvided_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(
                () => NewFileDownloadDataStoreProvider(null, databaseConnectionServiceFactoryMock, loggingServiceMock));
            Assert.Throws<ArgumentNullException>(
                () => NewFileDownloadDataStoreProvider(databaseConnectionSettingsServiceMock, null, loggingServiceMock));
            Assert.Throws<ArgumentNullException>(
                () =>
                NewFileDownloadDataStoreProvider(
                    databaseConnectionSettingsServiceMock,
                    databaseConnectionServiceFactoryMock,
                    null));
        }

        [Test]
        public void FileDownloadFinished_WhenInvoked_FileDataUpload()
        {
            InvokeFileDownloadFinished();

            Received.InOrder(
                () =>
                    {
                        loggingServiceMock.LogVerbose("FileDownloadFinished Invoked");
                        databaseConnectionServiceMock.Open();
                        databaseConnectionServiceMock.ExecuteStoredProcedure(
                            "[FoldingCoin].[FileDownloadFinished]",
                            Arg.Any<List<DbParameter>>());
                        databaseConnectionServiceMock.Close();
                    });
        }

        [Test]
        public void FileDownloadFinished_WhenInvoked_ParametersAreProvided()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                service =>
                service.ExecuteStoredProcedure("[FoldingCoin].[FileDownloadFinished]", Arg.Any<List<DbParameter>>()))
                .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            InvokeFileDownloadFinished();

            Assert.That(actualParameters.Count, Is.EqualTo(4));
            Assert.That(actualParameters[0].ParameterName, Is.EqualTo("@DownloadId"));
            Assert.That(actualParameters[0].DbType, Is.EqualTo(DbType.Int32));
            Assert.That(actualParameters[0].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[0].Value, Is.EqualTo(100));
            Assert.That(actualParameters[1].ParameterName, Is.EqualTo("@FileName"));
            Assert.That(actualParameters[1].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[1].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[1].Value, Is.EqualTo("UncompressedDownloadFileName"));
            Assert.That(actualParameters[2].ParameterName, Is.EqualTo("@FileExtension"));
            Assert.That(actualParameters[2].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[2].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[2].Value, Is.EqualTo("UncompressedDownloadFileExtension"));
            Assert.That(actualParameters[3].ParameterName, Is.EqualTo("@FileData"));
            Assert.That(actualParameters[3].DbType, Is.EqualTo(DbType.String));
            Assert.That(actualParameters[3].Direction, Is.EqualTo(ParameterDirection.Input));
            Assert.That(actualParameters[3].Value, Is.EqualTo("UncompressedDownloadFileData"));
        }

        [Test]
        public void GetLastFileDownloadDateTime_WhenInvoked_GetsLastfileDownloadDateTime()
        {
            InvokeGetLastFileFownloadDateTime();

            Received.InOrder(
                () =>
                    {
                        loggingServiceMock.LogVerbose("GetLastFileDownloadDateTime Invoked");
                        databaseConnectionServiceMock.Open();
                        databaseConnectionServiceMock.ExecuteScalar(
                            "SELECT [FoldingCoin].[GetLastFileDownloadDateTime]()");
                        databaseConnectionServiceMock.Close();
                    });
        }

        [Test]
        public void GetLastFileDownloadDateTime_WhenInvoked_ReturnsDateTime()
        {
            DateTime dateTime = DateTime.Now;
            databaseConnectionServiceMock.ExecuteScalar("SELECT [FoldingCoin].[GetLastFileDownloadDateTime]()")
                .Returns(dateTime);

            DateTime actual = InvokeGetLastFileFownloadDateTime();

            Assert.That(actual, Is.EqualTo(dateTime));
        }

        [Test]
        public void GetLastFileDownloadDateTime_WhenNoRowsReturned_ReturnsDefaultDateTime()
        {
            DateTime actual = InvokeGetLastFileFownloadDateTime();

            Assert.That(actual, Is.EqualTo(default(DateTime)));
        }

        [Test]
        public void IsAvailable_Invoked_ConnectionOpenedThenClosed()
        {
            InvokeIsAvailable();

            Received.InOrder(
                (() =>
                    {
                        loggingServiceMock.LogVerbose("IsAvailable Invoked");
                        databaseConnectionServiceMock.Open();
                        loggingServiceMock.LogVerbose("Database connection was successful");
                        databaseConnectionServiceMock.Close();
                    }));
        }

        [Test]
        public void IsAvailable_WhenDatabaseConnectionFails_ConnectionOpenedThenClosed()
        {
            var expected = new Exception();
            databaseConnectionServiceMock.When(mock => mock.Open()).Throw(expected);

            InvokeIsAvailable();

            Received.InOrder(
                (() =>
                    {
                        loggingServiceMock.LogVerbose("IsAvailable Invoked");
                        databaseConnectionServiceMock.Open();
                        databaseConnectionServiceMock.Close();
                        loggingServiceMock.LogException(expected);
                    }));
        }

        [Test]
        public void IsAvailable_WhenDatabaseConnectionFails_ReturnsFalse()
        {
            databaseConnectionServiceMock.When(mock => mock.Open()).Throw<Exception>();

            bool actual = InvokeIsAvailable();

            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsAvailable_WhenInvoked_ReturnsTrue()
        {
            bool actual = InvokeIsAvailable();

            Assert.That(actual, Is.True);
        }

        [Test]
        public void NewFileDownloadStarted_WhenInvoked_NewFileDownloadStarted()
        {
            InvokeNewFileDownloadStarted();

            Received.InOrder(
                (() =>
                    {
                        loggingServiceMock.LogVerbose("NewFileDownloadStarted Invoked");
                        databaseConnectionServiceMock.Open();
                        loggingServiceMock.LogVerbose("Database connection was successful");
                        databaseConnectionServiceMock.ExecuteStoredProcedure(
                            "[FoldingCoin].[NewFileDownloadStarted]",
                            Arg.Any<List<DbParameter>>());
                        databaseConnectionServiceMock.Close();
                    }));
        }

        [Test]
        public void NewFileDownloadStarted_WhenInvoked_ParametersAreProvided()
        {
            List<DbParameter> actualParameters = default(List<DbParameter>);

            databaseConnectionServiceMock.When(
                service =>
                service.ExecuteStoredProcedure("[FoldingCoin].[NewFileDownloadStarted]", Arg.Any<List<DbParameter>>()))
                .Do(callback => { actualParameters = callback.Arg<List<DbParameter>>(); });

            InvokeNewFileDownloadStarted();

            Assert.That(actualParameters.Count, Is.EqualTo(1));
            Assert.That(actualParameters[0].ParameterName, Is.EqualTo("@DownloadId"));
            Assert.That(actualParameters[0].DbType, Is.EqualTo(DbType.Int32));
            Assert.That(actualParameters[0].Direction, Is.EqualTo(ParameterDirection.Output));
        }

        [Test]
        public void NewFileDownloadStarted_WhenInvoked_ReturnsDownloadId()
        {
            var dbParameter = Substitute.For<DbParameter>();
            dbParameter.Value.Returns(101);

            databaseConnectionServiceMock.ClearSubstitute();
            databaseConnectionServiceMock.CreateParameter("@DownloadId", DbType.Int32, ParameterDirection.Output)
                .Returns(dbParameter);

            InvokeNewFileDownloadStarted();

            Assert.That(filePayload.DownloadId, Is.EqualTo(101));
        }

        [SetUp]
        public void SetUp()
        {
            databaseConnectionSettingsServiceMock = Substitute.For<IDatabaseConnectionSettingsService>();
            databaseConnectionSettingsServiceMock.GetConnectionString().Returns("connectionString");

            databaseConnectionServiceMock = Substitute.For<IDatabaseConnectionService>();
            databaseConnectionServiceFactoryMock = Substitute.For<IDatabaseConnectionServiceFactory>();
            databaseConnectionServiceFactoryMock.Create("connectionString").Returns(databaseConnectionServiceMock);

            loggingServiceMock = Substitute.For<ILoggingService>();

            systemUnderTest = NewFileDownloadDataStoreProvider(
                databaseConnectionSettingsServiceMock,
                databaseConnectionServiceFactoryMock,
                loggingServiceMock);

            databaseConnectionServiceMock.CreateParameter(
                Arg.Any<string>(),
                Arg.Any<DbType>(),
                Arg.Any<ParameterDirection>()).Returns(
                    info =>
                        {
                            var parameterName = info.Arg<string>();
                            var dbType = info.Arg<DbType>();
                            var direction = info.Arg<ParameterDirection>();

                            var dbParameter = Substitute.For<DbParameter>();
                            dbParameter.ParameterName.Returns(parameterName);
                            dbParameter.DbType.Returns(dbType);
                            dbParameter.Direction.Returns(direction);

                            if (dbType.Equals(DbType.Int32))
                            {
                                dbParameter.Value.Returns(default(int));
                            }

                            return dbParameter;
                        });

            filePayload = new FilePayload
                              {
                                  DownloadId = 100, UncompressedDownloadFileName = "UncompressedDownloadFileName",
                                  UncompressedDownloadFileExtension = "UncompressedDownloadFileExtension",
                                  UncompressedDownloadFileData = "UncompressedDownloadFileData"
                              };
        }

        [Test]
        public void UpdateToLatest_WhenInvoked_DatabaseUpdatedToLatest()
        {
            databaseConnectionServiceMock.ExecuteStoredProcedure(Arg.Any<string>())
                .Returns(NumberOfRowsEffectedExpected);

            InvokeUpdateToLatest();

            Received.InOrder(
                (() =>
                    {
                        loggingServiceMock.LogVerbose("UpdateToLatest Invoked");
                        databaseConnectionServiceMock.Open();
                        loggingServiceMock.LogVerbose("Database connection was successful");
                        databaseConnectionServiceMock.ExecuteStoredProcedure("[FoldingCoin].[UpdateToLatest]");
                        loggingServiceMock.LogVerbose($"'{NumberOfRowsEffectedExpected}' rows were effected");
                        databaseConnectionServiceMock.Close();
                    }));
        }

        private void InvokeFileDownloadFinished()
        {
            systemUnderTest.FileDownloadFinished(filePayload);
        }

        private DateTime InvokeGetLastFileFownloadDateTime()
        {
            return systemUnderTest.GetLastFileDownloadDateTime();
        }

        private bool InvokeIsAvailable()
        {
            return systemUnderTest.IsAvailable();
        }

        private void InvokeNewFileDownloadStarted()
        {
            systemUnderTest.NewFileDownloadStarted(filePayload);
        }

        private void InvokeUpdateToLatest()
        {
            systemUnderTest.UpdateToLatest();
        }

        private IFileDownloadDataStoreService NewFileDownloadDataStoreProvider(
            IDatabaseConnectionSettingsService databaseConnectionSettingsService,
            IDatabaseConnectionServiceFactory databaseConnectionServiceFactory,
            ILoggingService loggingService)
        {
            return new FileDownloadDataStoreProvider(
                databaseConnectionSettingsService,
                databaseConnectionServiceFactory,
                loggingService);
        }
    }
}