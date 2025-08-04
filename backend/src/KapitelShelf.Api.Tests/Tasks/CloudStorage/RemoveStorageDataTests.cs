// <copyright file="RemoveStorageDataTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.Logic.CloudStorages;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Tasks.CloudStorage;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;

namespace KapitelShelf.Api.Tests.Tasks.CloudStorage;

/// <summary>
/// Unit tests for RemoveStorageData.
/// </summary>
[TestFixture]
public class RemoveStorageDataTests
{
    private RemoveStorageData testee;
    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private ICloudStoragesLogic logic;
    private IJobExecutionContext context;

    /// <summary>
    /// Sets up testee and dependencies.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Setup
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.logic = Substitute.For<ICloudStoragesLogic>();
        this.context = Substitute.For<IJobExecutionContext>();

        var jobKey = new JobKey("UnitTestJob", "TestGroup");
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(jobKey);
        this.context.JobDetail.Returns(jobDetail);

        this.testee = new RemoveStorageData(this.dataStore, this.logger, this.logic);
    }

    /// <summary>
    /// Tests ExecuteTask does nothing if storage type is invalid.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_Returns_IfInvalidStorageType()
    {
        // Setup
        this.testee.StorageOwnerEmail = "test@example.com";
        this.testee.StorageType = "notARealType";

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        this.logic.DidNotReceiveWithAnyArgs().DeleteStorageData(Arg.Any<CloudStorageDTO>(), Arg.Any<bool>(), Arg.Any<Action<string, int, int>?>());
    }

    /// <summary>
    /// Tests ExecuteTask calls DeleteStorageData with correct arguments.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <returns>A task.</returns>
    [Test]
    [TestCaseSource(nameof(AllCloudTypes))]
    public async Task ExecuteTask_CallsDeleteStorageData(CloudTypeDTO cloudType)
    {
        // Setup
        this.testee.StorageOwnerEmail = "owner@example.com";
        this.testee.StorageType = cloudType.ToString();
        this.testee.RemoveOnlyCloudData = true;

        CloudStorageDTO? receivedStorage = null;
        bool? receivedRemoveOnlyCloud = null;
        Action<string, int, int>? receivedCallback = null;

        this.logic.When(x => x.DeleteStorageData(Arg.Any<CloudStorageDTO>(), Arg.Any<bool>(), Arg.Any<Action<string, int, int>>()))
            .Do(call =>
            {
                receivedStorage = call.ArgAt<CloudStorageDTO>(0);
                receivedRemoveOnlyCloud = call.ArgAt<bool>(1);
                receivedCallback = call.ArgAt<Action<string, int, int>>(2);
            });

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        Assert.That(receivedStorage, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(receivedStorage.CloudOwnerEmail, Is.EqualTo("owner@example.com"));
            Assert.That(receivedStorage.Type, Is.EqualTo(cloudType));
            Assert.That(receivedRemoveOnlyCloud, Is.True);
            Assert.That(receivedCallback, Is.Not.Null);
        });
    }

    /// <summary>
    /// Tests ExecuteTask sets executionContext for callback.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <returns>A task.</returns>
    [Test]
    [TestCaseSource(nameof(AllCloudTypes))]
    public async Task ExecuteTask_SetsExecutionContextForCallback(CloudTypeDTO cloudType)
    {
        // Setup
        this.testee.StorageOwnerEmail = "foo@example.com";
        this.testee.StorageType = cloudType.ToString();
        this.testee.RemoveOnlyCloudData = false;

        Action<string, int, int>? callback = null;

        this.logic.When(x => x.DeleteStorageData(Arg.Any<CloudStorageDTO>(), Arg.Any<bool>(), Arg.Any<Action<string, int, int>?>()))
            .Do(call => callback = call.ArgAt<Action<string, int, int>>(2));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        Assert.That(callback, Is.Not.Null);

        // call the callback, it should set progress using DataStore (and not throw)
        this.testee.executionContext = this.context;
        Assert.DoesNotThrow(() => callback.Invoke("foo.txt", 5, 2));

        this.dataStore.Received().SetProgress(Arg.Any<string>(), 2, 5);
    }

    /// <summary>
    /// Tests OnFileDelete throws if executionContext is null.
    /// </summary>
    [Test]
    public void OnFileDelete_Throws_IfNoExecutionContext() => Assert.Throws<ArgumentNullException>(() => this.testee.OnFileDelete("somefile", 3, 1));

    /// <summary>
    /// Tests Kill does nothing.
    /// </summary>
    [Test]
    public void Kill_DoesNothing() => Assert.DoesNotThrowAsync(this.testee.Kill);

    /// <summary>
    /// Tests Schedule creates and schedules the job.
    /// </summary>
    /// <param name="cloudType">The cloud type.</param>
    /// <returns>A task.</returns>
    [Test]
    [TestCaseSource(nameof(AllCloudTypes))]
    public async Task Schedule_CreatesAndSchedulesJob_ReturnsJobKey(CloudTypeDTO cloudType)
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var storage = new CloudStorageDTO
        {
            Id = Guid.NewGuid(),
            Type = cloudType,
            CloudOwnerEmail = "test@kapitelshelf.com",
            CloudOwnerName = "Test",
        };

        // Execute
        var jobKey = await RemoveStorageData.Schedule(scheduler, storage, true);

        // Assert
        Assert.That(jobKey, Is.Not.Null.And.Contain("Removing local cloud data"));
        await scheduler.Received().ScheduleJob(
            Arg.Any<IJobDetail>(),
            Arg.Any<ITrigger>());
    }

    /// <summary>
    /// Provides all valid CloudTypeDTO enum values.
    /// </summary>
    private static IEnumerable<CloudTypeDTO> AllCloudTypes() => Enum.GetValues<CloudTypeDTO>().Cast<CloudTypeDTO>();
}
