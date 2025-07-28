// <copyright file="SyncStorageDataTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Settings;
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
    private ICloudStorage fileStorage;
    private ICloudStoragesLogic logic;
    private IMapper mapper;
    private IProcessUtils processUtils;
    private IJobExecutionContext context;
    private CloudStorageDTO storage;
    private CloudStorageModel storageModel;

    /// <summary>
    /// Sets up testee and dependencies.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Setup
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.fileStorage = Substitute.For<ICloudStorage>();
        this.logic = Substitute.For<ICloudStoragesLogic>();
        this.mapper = Substitute.For<IMapper>();
        this.processUtils = Substitute.For<IProcessUtils>();
        this.context = Substitute.For<IJobExecutionContext>();

        // Setup job key
        var jobKey = new JobKey("UnitTestJob", "TestGroup");
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(jobKey);
        this.context.JobDetail.Returns(jobDetail);

        this.storage = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            CloudDirectory = "TestDir",
            Type = CloudTypeDTO.OneDrive,
        };

        this.storageModel = Substitute.For<CloudStorageModel>();
        this.mapper.Map<CloudStorageDTO>(this.storageModel).Returns(this.storage);

        this.testee = new SyncStorageData(
            this.dataStore,
            this.logger,
            this.logic,
            this.mapper);
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
        await this.processUtils.DidNotReceiveWithAnyArgs().RunProcessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<Action<Process>?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests ExecuteTask executes bisync for supported storages.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_ExecutesBisync_IfSupported()
    {
        // Setup
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        StaticConstants.StoragesSupportRCloneBisync.Clear();
        StaticConstants.StoragesSupportRCloneBisync.Add(CloudTypeDTO.OneDrive);

        this.logic.GetDownloadedStorageModels().Returns([this.storageModel]);
        this.fileStorage.FullPath(this.storage, Arg.Any<string>()).Returns(tempDir);

        this.processUtils.RunProcessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<Action<Process>?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((0, string.Empty, string.Empty)));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.processUtils.Received().RunProcessAsync(
            Arg.Any<string>(),
            Arg.Is<string>(args => args.Contains("bisync")),
            Arg.Any<string?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<Action<Process>?>(),
            Arg.Any<CancellationToken>());

        Directory.Delete(tempDir, true);
    }

    /// <summary>
    /// Tests ExecuteTask executes sync for non-bisync storages.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_ExecutesSync_IfNotBisync()
    {
        // Setup
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        StaticConstants.StoragesSupportRCloneBisync.Clear();

        this.logic.GetDownloadedStorageModels().Returns([this.storageModel]);
        this.fileStorage.FullPath(this.storage, Arg.Any<string>()).Returns(tempDir);

        this.processUtils.RunProcessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<Action<Process>?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((0, string.Empty, string.Empty)));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.processUtils.Received().RunProcessAsync(
            Arg.Any<string>(),
            Arg.Is<string>(args => args.Contains("sync")),
            Arg.Any<string?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<Action<string>?>(),
            Arg.Any<string?>(),
            Arg.Any<Action<Process>?>(),
            Arg.Any<CancellationToken>());

        Directory.Delete(tempDir, true);
    }

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
    /// Tests Kill does nothing if process is null.
    /// </summary>
    [Test]
    public void Kill_DoesNothing_IfNoProcess() =>
        Assert.DoesNotThrowAsync(this.testee.Kill);

    /// <summary>
    /// Tests Schedule creates and schedules the job.
    /// </summary>
    /// <returns>A task with the job key string.</returns>
    [Test]
    public async Task Schedule_CreatesAndSchedulesJob_ReturnsJobKey()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();

        // Execute
        var jobKey = await SyncStorageData.Schedule(scheduler);

        // Assert
        Assert.That(jobKey, Is.Not.Null.And.Contains("Sync Cloud Storages"));
        await scheduler.Received().ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<IReadOnlyCollection<ITrigger>>(),
            true,
            Arg.Any<CancellationToken>());
    }
}
