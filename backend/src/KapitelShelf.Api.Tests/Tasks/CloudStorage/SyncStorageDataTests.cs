// <copyright file="SyncStorageDataTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Logic.Interfaces.CloudStorages;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Tasks.CloudStorage;
using KapitelShelf.Api.Utils;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;

namespace KapitelShelf.Api.Tests.Tasks.CloudStorage;

/// <summary>
/// Unit tests for SyncStorageData.
/// </summary>
[TestFixture]
public class SyncStorageDataTests
{
    private SyncStorageData testee;
    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private INotificationsLogic notificationsLogic;
    private ICloudStoragesLogic logic;
    private Mapper mapper;
    private IJobExecutionContext context;

    /// <summary>
    /// Sets up testee and dependencies.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // Setup
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.notificationsLogic = Substitute.For<INotificationsLogic>();
        this.logic = Substitute.For<ICloudStoragesLogic>();
        this.mapper = Testhelper.CreateMapper();
        this.context = Substitute.For<IJobExecutionContext>();

        var jobKey = new JobKey("UnitTestJob", "TestGroup");
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(jobKey);
        this.context.JobDetail.Returns(jobDetail);

        this.testee = new SyncStorageData(this.dataStore, this.logger, this.notificationsLogic, this.logic, this.mapper);
    }

    /// <summary>
    /// Tests ExecuteTask returns if no storages are found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_Returns_IfNoStorages()
    {
        // Setup
        this.logic.GetDownloadedStorageModels().Returns([]);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.DidNotReceiveWithAnyArgs().SyncStorage(Arg.Any<CloudStorageDTO>(), Arg.Any<Action<string>?>(), Arg.Any<Action<Process>?>());
        this.dataStore.DidNotReceiveWithAnyArgs().SetMessage(Arg.Any<string>(), Arg.Any<string>());
    }

    /// <summary>
    /// Tests ExecuteTask syncs all storages if found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_SyncsAllStorages()
    {
        // Setup
        var storage = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            CloudDirectory = "TestDir",
            Type = CloudType.OneDrive,
        };

        var storages = new List<CloudStorageModel> { storage };
        this.logic.GetDownloadedStorageModels().Returns(storages);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received(1)
                .SyncStorage(Arg.Is<CloudStorageDTO>(x => x.Id == storage.Id), Arg.Any<Action<string>>(), Arg.Any<Action<Process>>());
        this.dataStore.Received().SetMessage(Arg.Any<string>(), Arg.Is<string>(msg => msg.Contains("Synching")));
    }

    /// <summary>
    /// Tests ExecuteTask syncs multiple storages.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_SyncsMultipleStorages()
    {
        // Setup
        var storage1 = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            CloudDirectory = "OtherDir",
            Type = CloudType.OneDrive,
        };
        var storage2 = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            CloudDirectory = "OtherDir2",
            Type = CloudType.OneDrive,
        };

        var storages = new List<CloudStorageModel> { storage1, storage2 };
        this.logic.GetDownloadedStorageModels().Returns(storages);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received(1)
            .SyncStorage(Arg.Is<CloudStorageDTO>(x => x.Id == storage1.Id), Arg.Any<Action<string>>(), Arg.Any<Action<Process>>());
        await this.logic.Received(1)
            .SyncStorage(Arg.Is<CloudStorageDTO>(x => x.Id == storage2.Id), Arg.Any<Action<string>>(), Arg.Any<Action<Process>>());
    }

    /// <summary>
    /// Tests ExecuteTask throws if GetDownloadedStorageModels fails.
    /// </summary>
    [Test]
    public void ExecuteTask_Throws_IfLogicThrows()
    {
        // Setup
        this.logic.GetDownloadedStorageModels().Returns(Task.FromException<List<CloudStorageModel>>(new ArgumentException("fail")));

        // Execute + Assert
        Assert.ThrowsAsync<ArgumentException>(() => this.testee.ExecuteTask(this.context));
    }

    /// <summary>
    /// Tests ExecuteTask syncs a single storage when ForSingleStorageId is set and found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_SyncsSingleStorage_IfForSingleStorageIdSet()
    {
        // Setup
        var storage = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            CloudDirectory = "OtherDir",
            Type = CloudType.OneDrive,
        };

        this.testee.ForSingleStorageId = storage.Id;
        this.logic.GetStorageModel(storage.Id).Returns(storage);

        var notificationId = Guid.NewGuid();
        this.notificationsLogic.AddNotification(
            Arg.Any<string>(),
            titleArgs: Arg.Any<object[]>(),
            messageArgs: Arg.Any<object[]>(),
            type: Arg.Any<NotificationTypeDto>(),
            severity: Arg.Any<NotificationSeverityDto>(),
            source: Arg.Any<string>(),
            disableAutoGrouping: Arg.Any<bool>())
            .Returns(Task.FromResult<List<NotificationDto>>([
                new NotificationDto { Id = notificationId }
            ]));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received(1)
            .SyncStorage(Arg.Is<CloudStorageDTO>(x => x.Id == storage.Id), Arg.Any<Action<string>>(), Arg.Any<Action<Process>>());
        this.dataStore.Received().SetMessage(Arg.Any<string>(), Arg.Is<string>(msg => msg.Contains("Synching")));
        _ = this.notificationsLogic.Received(1).AddNotification(
            "CloudStorageSingleSyncStarted",
            titleArgs: Arg.Any<object[]>(),
            messageArgs: Arg.Any<object[]>(),
            type: NotificationTypeDto.Info,
            severity: NotificationSeverityDto.Low,
            source: "Cloud Storage",
            disableAutoGrouping: true);
        _ = this.notificationsLogic.Received(1).AddNotification(
            "CloudStorageSingleSyncFinished",
            type: NotificationTypeDto.Success,
            severity: NotificationSeverityDto.Low,
            source: "Cloud Storage",
            parentId: notificationId);
    }

    /// <summary>
    /// Tests ExecuteTask skips single storage if not found.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_SkipsIfSingleStorageMissing()
    {
        // Setup
        var missingId = Guid.NewGuid();
        this.testee.ForSingleStorageId = missingId;
        this.logic.GetStorageModel(missingId).Returns((CloudStorageModel?)null);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.DidNotReceiveWithAnyArgs().SyncStorage(Arg.Any<CloudStorageDTO>(), Arg.Any<Action<string>?>(), Arg.Any<Action<Process>?>());
    }

    /// <summary>
    /// Tests ExecuteTask throws if GetStorageModel fails.
    /// </summary>
    [Test]
    public void ExecuteTask_SingleStorage_Throws_IfLogicThrows()
    {
        // Setup
        var id = Guid.NewGuid();
        this.testee.ForSingleStorageId = id;
        this.logic.GetStorageModel(id).Returns(Task.FromException<CloudStorageModel?>(new ArgumentException("fail")));

        // Execute + Assert
        Assert.ThrowsAsync<ArgumentException>(() => this.testee.ExecuteTask(this.context));
    }

    /// <summary>
    /// Tests DownloadProgressHandler sets progress and message for valid output.
    /// </summary>
    [Test]
    public void DownloadProgressHandler_SetsProgress_AndMessage()
    {
        // Setup
        this.testee.executionContext = this.context;
        this.testee.currentStorageIndex = 0;
        this.testee.totalStorageIndex = 1;

        var progressLine = /*lang=json,strict*/ "{\"time\":\"2025-09-17T14:59:44.3188582+02:00\",\"level\":\"notice\",\"msg\":\"\\nTransferred:   \\t   16.395 MiB / 16.395 MiB, 100%, 2.287 MiB/s, ETA 0s\\nChecks:                67 / 67, 100%, Listed 143\\nTransferred:            9 / 9, 100%\\nElapsed time:         3.2s\\n\\n\",\"stats\":{\"bytes\":17191001,\"checks\":67,\"deletedDirs\":0,\"deletes\":0,\"elapsedTime\":3.2597123,\"errors\":0,\"eta\":0,\"fatalError\":false,\"listed\":143,\"renames\":0,\"retryError\":false,\"serverSideCopies\":0,\"serverSideCopyBytes\":0,\"serverSideMoveBytes\":0,\"serverSideMoves\":0,\"speed\":2398167,\"totalBytes\":17191001,\"totalChecks\":67,\"totalTransfers\":9,\"transferTime\":2.6947928,\"transfers\":9},\"source\":\"slog/logger.go:256\"}";

        // Execute
        this.testee.DownloadProgressHandler(progressLine);

        // Assert
        this.dataStore.Received().SetProgress(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Is<int>(x => x >= 0 && x <= 100));
        this.dataStore.Received().SetMessage(Arg.Any<string>(), Arg.Is<string>(x => x.Contains("ETA")));
    }

    /// <summary>
    /// Tests DownloadProgressHandler ignores non-ETA lines.
    /// </summary>
    [Test]
    public void DownloadProgressHandler_IgnoresNonEtaLine()
    {
        // Setup
        this.testee.executionContext = this.context;

        // Execute
        this.testee.DownloadProgressHandler("no progress here");

        // Assert
        this.dataStore.DidNotReceiveWithAnyArgs().SetProgress(Arg.Any<string>(), Arg.Any<int>());
    }

    /// <summary>
    /// Tests DownloadProgressHandler handles malformed lines.
    /// </summary>
    [Test]
    public void DownloadProgressHandler_HandlesMalformedData()
    {
        // Setup
        this.testee.executionContext = this.context;

        // Execute
        this.testee.DownloadProgressHandler("random string, ETA ");

        // Assert
        this.dataStore.DidNotReceive().SetMessage(Arg.Any<string>(), Arg.Any<string>());
    }

    /// <summary>
    /// Tests DownloadProgressHandler throws if executionContext not set.
    /// </summary>
    [Test]
    public void DownloadProgressHandler_ThrowsIfNoExecutionContext() =>
        Assert.Throws<ArgumentNullException>(() => this.testee.DownloadProgressHandler("100.0 MiB / 200.0 MiB, 50%, 5.0 MiB/s, ETA 2m0s"));

    /// <summary>
    /// Tests Kill terminates rclone process if not exited.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Kill_TerminatesRcloneProcess_IfNotExited()
    {
        // Setup
        var process = Substitute.For<IProcess>();
        process.HasExited.Returns(false);

        this.testee.rcloneProcess = process;

        // Execute
        await this.testee.Kill();

        // Assert
        process.Received().Kill(entireProcessTree: true);
    }

    /// <summary>
    /// Tests Kill does nothing if process has exited.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Kill_DoesNothing_IfProcessHasExited()
    {
        // Setup
        var process = Substitute.For<IProcess>();
        process.HasExited.Returns(true);

        this.testee.rcloneProcess = process;

        // Execute
        await this.testee.Kill();

        // Assert
        process.DidNotReceive().Kill(Arg.Any<bool>());
    }

    /// <summary>
    /// Tests Kill does nothing if rcloneProcess is null.
    /// </summary>
    [Test]
    public void Kill_DoesNothing_IfNoProcess() =>
        Assert.DoesNotThrowAsync(this.testee.Kill);

    /// <summary>
    /// Tests Kill logs error if Kill throws.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Kill_LogsError_IfExceptionThrown()
    {
        // Setup
        var process = Substitute.For<IProcess>();
        process.HasExited.Returns(false);
        process.When(x => x.Kill(entireProcessTree: true)).Do(x => throw new InvalidOperationException("fail"));

        this.testee.rcloneProcess = process;

        // Execute
        await this.testee.Kill();

        // Assert
        this.logger.Received().LogError(Arg.Any<Exception>(), "Could not kill rclone process");
    }

    /// <summary>
    /// Tests Schedule creates and schedules the job for all storages.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Schedule_CreatesAndSchedulesJob_ReturnsJobKey()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();

        // Execute
        var jobKey = await SyncStorageData.Schedule(scheduler);

        // Assert
        Assert.That(jobKey, Is.Not.Null.And.Contain("Sync Cloud Storages"));
        await scheduler.Received().ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<IReadOnlyCollection<ITrigger>>(),
            true,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests Schedule creates and schedules the job for a single storage.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Schedule_SingleStorage_CreatesAndSchedulesJobWithCustomTitle()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var storage = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            Type = CloudTypeDTO.OneDrive,
            CloudDirectory = "Special",
        };

        // Execute
        var jobKey = await SyncStorageData.Schedule(scheduler, null, storage);

        // Assert
        Assert.That(jobKey, Is.Not.Null);
        await scheduler.Received().ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<IReadOnlyCollection<ITrigger>>(),
            true,
            Arg.Any<CancellationToken>());
    }
}
