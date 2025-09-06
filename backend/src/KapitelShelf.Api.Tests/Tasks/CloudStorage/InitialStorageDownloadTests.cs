// <copyright file="InitialStorageDownloadTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics;
using AutoMapper;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Tasks.CloudStorage;
using KapitelShelf.Api.Utils;
using KapitelShelf.Data.Models.CloudStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;

namespace KapitelShelf.Api.Tests.Tasks.CloudStorage;

/// <summary>
/// Unit tests for InitialStorageDownload.
/// </summary>
[TestFixture]
public class InitialStorageDownloadTests
{
    private InitialStorageDownload testee;
    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private ICloudStoragesLogic logic;
    private IMapper mapper;
    private IJobExecutionContext context;

    /// <summary>
    /// Sets up testee and dependencies.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.logic = Substitute.For<ICloudStoragesLogic>();
        this.mapper = Testhelper.CreateMapper();
        this.context = Substitute.For<IJobExecutionContext>();

        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(new JobKey("DownloadJob", "Group"));
        this.context.JobDetail.Returns(jobDetail);

        this.testee = new InitialStorageDownload(this.dataStore, this.logger, this.logic, this.mapper);
    }

    /// <summary>
    /// Tests ExecuteTask returns if storage is missing.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_Returns_IfStorageMissing()
    {
        // Setup
        var id = Guid.NewGuid();
        this.testee.StorageId = id;
        this.logic.GetStorageModel(id).Returns((CloudStorageModel?)null);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.DidNotReceiveWithAnyArgs().DownloadStorageInitially(default!, default!, default!, default);
    }

    /// <summary>
    /// Tests ExecuteTask cleans directory and starts download.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_CleansAndDownloads()
    {
        // Setup
        var storageModel = new CloudStorageModel
        {
            Id = Guid.NewGuid(),
            Type = CloudType.OneDrive,
            CloudDirectory = "Books",
        };

        this.testee.StorageId = storageModel.Id;
        this.logic.GetStorageModel(storageModel.Id).Returns(storageModel);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.logic.Received().CleanStorageDirectory(Arg.Is<CloudStorageDTO>(s => s.Id == storageModel.Id));
        await this.logic.Received().DownloadStorageInitially(
            Arg.Any<CloudStorageDTO>(),
            Arg.Any<Action<string>>(),
            Arg.Any<Action<Process>>(),
            this.context.CancellationToken);
        this.dataStore.Received().SetProgress(Arg.Any<string>(), 20);
    }

    /// <summary>
    /// Tests Kill does nothing if process is null.
    /// </summary>
    [Test]
    public void Kill_DoesNothing_IfNoProcess() => Assert.DoesNotThrowAsync(this.testee.Kill);

    /// <summary>
    /// Tests Kill terminates process if running.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Kill_TerminatesProcess_IfRunning()
    {
        var process = Substitute.For<IProcess>();
        process.HasExited.Returns(false);
        this.testee.GetType().GetField("rcloneProcess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(this.testee, process);

        await this.testee.Kill();

        process.Received().Kill(entireProcessTree: true);
    }

    /// <summary>
    /// Tests Kill logs error if process.Kill throws.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Kill_LogsError_IfKillFails()
    {
        var process = Substitute.For<IProcess>();
        process.HasExited.Returns(false);
        process.When(x => x.Kill(entireProcessTree: true)).Do(x => throw new InvalidOperationException("fail"));

        this.testee.GetType().GetField("rcloneProcess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(this.testee, process);

        await this.testee.Kill();

        this.logger.Received().LogError(Arg.Any<Exception>(), "Could not kill rclone process");
    }

    /// <summary>
    /// Tests DownloadProgressHandler sets progress and message.
    /// </summary>
    [Test]
    public void DownloadProgressHandler_SetsProgressAndMessage()
    {
        this.testee.GetType().GetField("executionContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(this.testee, this.context);

        var line = "100.0 MiB / 200.0 MiB, 50%, 5.0 MiB/s, ETA 2m0s";

        this.testee.GetType().GetMethod("DownloadProgressHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(this.testee, [line]);

        this.dataStore.Received().SetProgress(Arg.Any<string>(), Arg.Is<int>(x => x >= 20 && x <= 100));
        this.dataStore.Received().SetMessage(Arg.Any<string>(), Arg.Is<string>(msg => msg.Contains("ETA")));
    }

    /// <summary>
    /// Tests Schedule creates and schedules the job.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Schedule_CreatesAndSchedulesJob()
    {
        var scheduler = Substitute.For<IScheduler>();
        var storage = new CloudStorageDTO { Id = Guid.NewGuid(), Type = CloudTypeDTO.OneDrive, CloudDirectory = "One" };

        var jobKey = await InitialStorageDownload.Schedule(scheduler, storage);

        Assert.That(jobKey, Is.Not.Null.And.Contain("Downloading cloud data"));
        await scheduler.Received().ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>(), Arg.Any<CancellationToken>());
    }
}
