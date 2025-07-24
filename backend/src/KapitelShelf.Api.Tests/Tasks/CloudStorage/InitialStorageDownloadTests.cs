// <copyright file="InitialStorageDownloadTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Extensions;
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
/// UnitTests fot the InitialStorageDownload class.
/// </summary>
[TestFixture]
public class InitialStorageDownloadTests
{
    private InitialStorageDownload testee;
    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private ICloudStorage fileStorage;
    private ICloudStoragesLogic logic;
    private ISchedulerFactory schedulerFactory;
    private IMapper mapper;
    private KapitelShelfSettings settings;
    private IJobExecutionContext context;

    private CloudStorageDTO storage;
    private Guid storageId;

    /// <summary>
    /// Sets up the testee and dependencies.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.fileStorage = Substitute.For<ICloudStorage>();
        this.logic = Substitute.For<ICloudStoragesLogic>();
        this.schedulerFactory = Substitute.For<ISchedulerFactory>();
        this.mapper = Substitute.For<IMapper>();
        this.settings = new KapitelShelfSettings
        {
            CloudStorage = new CloudStorageSettings { RClone = "rclone" },
        };
        this.context = Substitute.For<IJobExecutionContext>();

        this.storageId = Guid.NewGuid();
        this.storage = new CloudStorageDTO
        {
            Id = storageId,
            CloudDirectory = "TestDir",
            Type = CloudTypeDTO.OneDrive,
        };

        this.testee = new InitialStorageDownload(
            this.dataStore,
            this.logger,
            this.fileStorage,
            this.logic,
            this.schedulerFactory,
            this.mapper,
            this.settings)
        {
            StorageId = storageId,
        };
    }

    /// <summary>
    /// Tests ExecuteTask returns when storage does not exist.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_Returns_IfStorageNotFound()
    {
        // Setup
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        this.logic.GetStorageModel(this.storageId).Returns((CloudStorageModel)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.DidNotReceive().MarkStorageAsDownloaded(this.storageId);
    }

    /// <summary>
    /// Tests ExecuteTask executes download and marks as downloaded.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_DownloadsAndMarksAsDownloaded()
    {
        // Setup
        var storageModel = Substitute.For<CloudStorageModel>();
        this.logic.GetStorageModel(this.storageId).Returns(storageModel);
        this.mapper.Map<CloudStorageDTO>(storageModel).Returns(this.storage);

        this.fileStorage.FullPath(this.storage, Arg.Any<string>()).Returns("TestPath");
        this.fileStorage.FullPath(this.storage).Returns("TestPath");

        Directory.CreateDirectory("TestPath"); // Make sure the directory exists for the test

        // Simulate sync command for non-bisync
        StaticConstants.StoragesSupportRCloneBisync.Clear(); // For sync, not bisync
        storageModel.ExecuteRCloneCommand(
            Arg.Any<string>(),
            Arg.Any<List<string>>(),
            Arg.Any<Action<string>>(),
            Arg.Any<Action<string>>(),
            Arg.Any<string>(),
            Arg.Any<Action<Process>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received().MarkStorageAsDownloaded(this.storage.Id);

        // Cleanup
        Directory.Delete("TestPath", true);
    }

    /// <summary>
    /// Tests Kill terminates running process if exists.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Kill_TerminatesRcloneProcess_IfNotExited()
    {
        // Setup
        var process = Substitute.For<IProcess>();
        process.HasExited.Returns(false);
        this.testee.GetType()
            .GetField("rcloneProcess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(this.testee, process);

        // Execute
        await this.testee.Kill();

        // Assert
        process.Received().Kill(entireProcessTree: true);
    }

    /// <summary>
    /// Tests Kill does nothing if process is null.
    /// </summary>
    [Test]
    public void Kill_DoesNothing_IfNoProcess() => Assert.DoesNotThrowAsync(this.testee.Kill);

    /// <summary>
    /// Tests Schedule creates and schedules the job.
    /// </summary>
    /// <returns>The job key string.</returns>
    [Test]
    public async Task Schedule_CreatesAndSchedulesJob_ReturnsJobKey()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var storage = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            CloudDirectory = "dir",
            Type = CloudTypeDTO.OneDrive,
        };

        // Execute
        var jobKey = await InitialStorageDownload.Schedule(scheduler, storage);

        // Assert
        Assert.That(jobKey, Is.Not.Null.And.Contains("Downloading cloud data"));
        await scheduler.Received().ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>());
    }
}
