// <copyright file="RemoveStorageDataTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.Storage;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Tasks.CloudStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;

namespace KapitelShelf.Api.Tests.Tasks.CloudStorage;

/// <summary>
/// UnitTests for the RemoveStorageData class.
/// </summary>
[TestFixture]
public class RemoveStorageDataTests
{
    private RemoveStorageData testee;
    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private ICloudStorage fileStorage;
    private IJobExecutionContext context;

    private string testDir;

    /// <summary>
    /// Sets up the testee and dependencies.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.fileStorage = Substitute.For<ICloudStorage>();
        this.context = Substitute.For<IJobExecutionContext>();

        // set up job key
        var jobKey = new JobKey("RemoveJob", "RemoveGroup");
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(jobKey);
        this.context.JobDetail.Returns(jobDetail);

        // create a test directory with files
        this.testDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(this.testDir);
        File.WriteAllText(Path.Combine(this.testDir, "file1.txt"), "Hello1");
        File.WriteAllText(Path.Combine(this.testDir, "file2.txt"), "Hello2");

        // substitute FullPath to return our testDir
        this.fileStorage.FullPath(Arg.Any<CloudStorageDTO>(), Arg.Any<string>()).Returns(callInfo =>
        {
            // support possible subpath logic
            var subPath = callInfo.ArgAt<string>(1);
            return string.IsNullOrEmpty(subPath) ? this.testDir : Path.Combine(this.testDir, subPath);
        });

        this.testee = new RemoveStorageData(this.dataStore, this.logger, this.fileStorage)
        {
            StorageOwnerEmail = "email@test.com",
            StorageType = CloudTypeDTO.OneDrive.ToString(),
            RemoveOnlyCloudData = false,
        };
    }

    /// <summary>
    /// Cleans up any created test directories.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.testDir))
        {
            Directory.Delete(this.testDir, true);
        }
    }

    /// <summary>
    /// Tests ExecuteTask deletes all files and directory when present.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Test]
    public async Task ExecuteTask_DeletesAllFilesAndDirectory_WhenPresent()
    {
        // Setup
        // Directory and files already created in SetUp

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        Assert.That(Directory.Exists(this.testDir), Is.False);
        this.dataStore.Received().SetProgress(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
    }

    /// <summary>
    /// Tests ExecuteTask returns if directory does not exist.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Test]
    public async Task ExecuteTask_Returns_IfDirectoryNotExists()
    {
        // Setup
        Directory.Delete(this.testDir, true);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        this.dataStore.DidNotReceive().SetProgress(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
    }

    /// <summary>
    /// Tests ExecuteTask skips files that fail to delete and logs error.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Test]
    public async Task ExecuteTask_LogsError_IfFileDeleteFails()
    {
        // Setup
        // lock the first file so it cannot be deleted
        var fileToLock = Path.Combine(this.testDir, "file1.txt");
        using var stream = File.Open(fileToLock, FileMode.Open, FileAccess.Read, FileShare.None);

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        this.logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o != null && o.ToString()!.Contains("Could not delete file") && o.ToString()!.Contains(fileToLock)),
            Arg.Any<IOException>(),
            Arg.Any<Func<object, Exception?, string>>());

        Assert.Multiple(() =>
        {
            // other file should still be deleted, and directory may not be deleted due to undeletable file
            Assert.That(File.Exists(fileToLock), Is.True);
            Assert.That(Directory.Exists(this.testDir), Is.True);
        });

        stream.Dispose();
    }

    /// <summary>
    /// Tests ExecuteTask returns if storage type is invalid.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Test]
    public async Task ExecuteTask_Returns_IfStorageTypeInvalid()
    {
        // Setup
        this.testee.StorageType = "NonExistingType";

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        // Should not try to delete anything
        this.dataStore.DidNotReceive().SetProgress(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
        Assert.That(Directory.Exists(this.testDir), Is.True);
    }

    /// <summary>
    /// Tests Kill does nothing.
    /// </summary>
    [Test]
    public void Kill_DoesNothing() => Assert.DoesNotThrowAsync(this.testee.Kill);

    /// <summary>
    /// Tests Schedule creates and schedules the job.
    /// </summary>
    /// <returns>A task representing the async operation. Returns the job key string.</returns>
    [Test]
    public async Task Schedule_CreatesAndSchedulesJob_ReturnsJobKey()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var storage = new CloudStorageDTO
        {
            CloudOwnerEmail = "email@test.com",
            CloudOwnerName = "Owner",
            Type = CloudTypeDTO.OneDrive,
        };

        // Execute
        var jobKey = await RemoveStorageData.Schedule(scheduler, storage);

        // Assert
        Assert.That(jobKey, Is.Not.Null.And.Contains("Removing local cloud data"));
        await scheduler.Received().ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>());
    }
}
