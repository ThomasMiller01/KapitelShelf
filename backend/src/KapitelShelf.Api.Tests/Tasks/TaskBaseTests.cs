// <copyright file="TaskBaseTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;

namespace KapitelShelf.Api.Tests.Tasks;

/// <summary>
/// UnitTests for the TaskBase class.
/// </summary>
[TestFixture]
public class TaskBaseTests
{
    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private IJobExecutionContext context;
    private TestTaskBase testee;

    /// <summary>
    /// Tests SetUp configures a valid test instance.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.context = Substitute.For<IJobExecutionContext>();

        // job key setup
        var jobKey = new JobKey("UnitTestJob", "TestGroup");
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(jobKey);
        this.context.JobDetail.Returns(jobDetail);

        this.testee = new TestTaskBase(this.dataStore, this.logger);
    }

    /// <summary>
    /// Tests Execute calls ExecuteTask, manages progress, and calls ClearData.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task Execute_CallsExecuteTaskAndManagesProgressAndCleanup()
    {
        // Setup
        this.testee.OnExecuteTask = _ => Task.CompletedTask;

        // Execute
        await this.testee.Execute(this.context);

        // Assert
        this.dataStore.Received().SetProgress("TestGroup.UnitTestJob", 0);
        this.dataStore.Received().ClearData("TestGroup.UnitTestJob");
        Assert.That(this.testee.ExecuteTaskCalled, Is.True);
    }

    /// <summary>
    /// Tests Execute handles OperationCanceledException and logs warning.
    /// </summary>
    [Test]
    public void Execute_HandlesOperationCanceledException()
    {
        // Setup
        this.testee.OnExecuteTask = _ => throw new OperationCanceledException();

        // Execute and Assert
        Assert.DoesNotThrowAsync(async () => await this.testee.Execute(this.context));
        this.dataStore.DidNotReceive().ClearData(Arg.Any<string>());
    }

    /// <summary>
    /// Tests Execute handles generic exception and logs error.
    /// </summary>
    [Test]
    public void Execute_HandlesGenericException()
    {
        // Setup
#pragma warning disable CA2201 // Do not raise reserved exception types
        this.testee.OnExecuteTask = _ => throw new Exception("Test error");
#pragma warning restore CA2201 // Do not raise reserved exception types

        // Execute and Assert
        Assert.DoesNotThrowAsync(async () => await this.testee.Execute(this.context));
        this.logger.Received().LogError(Arg.Any<Exception>(), "Error during task execution");
        this.dataStore.DidNotReceive().ClearData(Arg.Any<string>());
    }

    /// <summary>
    /// Tests Execute rethrows JobExecutionException.
    /// </summary>
    [Test]
    public void Execute_RethrowsJobExecutionException()
    {
        // Setup
        this.testee.OnExecuteTask = _ => throw new JobExecutionException();

        // Execute and Assert
        Assert.ThrowsAsync<JobExecutionException>(async () => await this.testee.Execute(this.context));
    }

    /// <summary>
    /// Tests JobKey returns correct job key string.
    /// </summary>
    [Test]
    public void JobKey_ReturnsCorrectKey()
    {
        // Execute
        var result = TaskBase.JobKey(this.context);

        // Assert
        Assert.That(result, Is.EqualTo("TestGroup.UnitTestJob"));
    }

    /// <summary>
    /// Tests CheckForInterrupt throws OperationCanceledException if token cancelled.
    /// </summary>
    [Test]
    public void CheckForInterrupt_ThrowsIfCancelled()
    {
        // Setup
        var cancelledContext = Substitute.For<IJobExecutionContext>();
        var jobKey = new JobKey("Test", "Test");
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(jobKey);
        cancelledContext.JobDetail.Returns(jobDetail);

        var cts = new CancellationTokenSource();
        cts.Cancel();
        cancelledContext.CancellationToken.Returns(cts.Token);

        // Execute and Assert
        Assert.Throws<OperationCanceledException>(() => this.testee.CheckForInterrupt(cancelledContext));
    }

    /// <summary>
    /// Tests JobKey and CheckForInterrupt throw ArgumentNullException if context is null.
    /// </summary>
    [Test]
    public void JobKey_ThrowsArgumentNullException_IfNull() => Assert.Throws<ArgumentNullException>(() => TaskBase.JobKey(null!));

    /// <summary>
    /// Tests Kill completes without error.
    /// </summary>
    [Test]
    public void Kill_DoesNothing() => Assert.DoesNotThrowAsync(this.testee.Kill);

    /// <summary>
    /// Tests PreScheduleSteps and PostScheduleSteps do not throw if options are null.
    /// </summary>
    [Test]
    public void PreAndPostScheduleSteps_DoNotThrowIfOptionsNull()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var job = Substitute.For<IJobDetail>();

        // Execute and Assert
        Assert.DoesNotThrowAsync(async () => await TaskBase.PreScheduleSteps(scheduler, job, null));
        Assert.DoesNotThrowAsync(async () => await TaskBase.PostScheduleSteps(scheduler, job, null));
    }

    /// <summary>
    /// Tests InterruptAndWait completes if job is not running (already finished).
    /// </summary>
    [Test]
    public void InterruptAndWait_JobNotRunning_CompletesImmediately()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var job = Substitute.For<IJobDetail>();
        var jobKey = new JobKey("JobA", "GroupA");
        job.Key.Returns(jobKey);

        scheduler.Interrupt(jobKey).Returns(Task.FromResult(false));
        scheduler.GetCurrentlyExecutingJobs().Returns(Task.FromResult<IReadOnlyCollection<IJobExecutionContext>>([]));

        // Execute & Assert
        Assert.DoesNotThrowAsync(async () => await TaskBase.InterruptAndWait(scheduler, job, 2));
    }

    /// <summary>
    /// Tests InterruptAndWait waits for job to finish within timeout.
    /// </summary>
    [Test]
    public void InterruptAndWait_JobRunning_FinishesWithinTimeout()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var job = Substitute.For<IJobDetail>();
        var jobKey = new JobKey("JobB", "GroupB");
        job.Key.Returns(jobKey);

        var context = Substitute.For<IJobExecutionContext>();
        context.JobDetail.Returns(job);

        // first call: running, second call: finished
        scheduler.Interrupt(jobKey).Returns(Task.FromResult(false));
        scheduler.GetCurrentlyExecutingJobs()
            .Returns(
                Task.FromResult<IReadOnlyCollection<IJobExecutionContext>>([context]),
                Task.FromResult<IReadOnlyCollection<IJobExecutionContext>>([]));

        // Execute & Assert
        Assert.DoesNotThrowAsync(async () => await TaskBase.InterruptAndWait(scheduler, job, 2));
    }

    /// <summary>
    /// Tests InterruptAndWait calls Kill if job does not finish in time and JobInstance is TaskBase.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task InterruptAndWait_JobDoesNotFinish_CallsKillOnTaskBase()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var job = Substitute.For<IJobDetail>();
        var jobKey = new JobKey("JobC", "GroupC");
        job.Key.Returns(jobKey);

        var taskBase = Substitute.For<TaskBase>(Substitute.For<ITaskRuntimeDataStore>(), Substitute.For<ILogger<TaskBase>>());
        var context = Substitute.For<IJobExecutionContext>();
        context.JobDetail.Returns(job);
        context.JobInstance.Returns(taskBase);

        // job never finishes
        scheduler.Interrupt(jobKey).Returns(Task.FromResult(false));
        scheduler.GetCurrentlyExecutingJobs().Returns(Task.FromResult<IReadOnlyCollection<IJobExecutionContext>>([context]));

        // Run InterruptAndWait with a very short timeout so it triggers Kill
        Assert.DoesNotThrowAsync(async () => await TaskBase.InterruptAndWait(scheduler, job, 1));

        // Assert Kill was called
        await taskBase.Received().Kill();
    }

    /// <summary>
    /// Tests InterruptAndWait does not throw or call Kill if JobInstance is not TaskBase.
    /// </summary>
    [Test]
    public void InterruptAndWait_JobDoesNotFinish_NoKillIfNotTaskBase()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var job = Substitute.For<IJobDetail>();
        var jobKey = new JobKey("JobD", "GroupD");
        job.Key.Returns(jobKey);

        var dummyJob = Substitute.For<IJob>();
        var context = Substitute.For<IJobExecutionContext>();
        context.JobDetail.Returns(job);
        context.JobInstance.Returns(dummyJob);

        scheduler.Interrupt(jobKey).Returns(Task.FromResult(false));
        scheduler.GetCurrentlyExecutingJobs().Returns(Task.FromResult<IReadOnlyCollection<IJobExecutionContext>>([context]));

        // Run InterruptAndWait with a short timeout
        Assert.DoesNotThrowAsync(async () => await TaskBase.InterruptAndWait(scheduler, job, 1));
    }

    /// <summary>
    /// Tests InterruptAndWait throws ArgumentNullException if scheduler or job is null.
    /// </summary>
    [Test]
    public void InterruptAndWait_ThrowsIfNullArgs()
    {
        // Assert
        var job = Substitute.For<IJobDetail>();
        Assert.ThrowsAsync<NullReferenceException>(async () => await TaskBase.InterruptAndWait(null!, job, 1));

        var scheduler = Substitute.For<IScheduler>();
        Assert.ThrowsAsync<NullReferenceException>(async () => await TaskBase.InterruptAndWait(scheduler, null!, 1));
    }

    /// <summary>
    /// Dummy TaskBase implementation.
    /// </summary>
    private sealed class TestTaskBase(ITaskRuntimeDataStore dataStore, ILogger<TaskBase> logger) : TaskBase(dataStore, logger)
    {
#pragma warning disable SA1401 // Fields should be private
        public Func<IJobExecutionContext, Task> OnExecuteTask = _ => Task.CompletedTask;
#pragma warning restore SA1401 // Fields should be private

        public bool ExecuteTaskCalled { get; private set; }

        /// <inheritdoc />
        public override Task ExecuteTask(IJobExecutionContext context)
        {
            this.ExecuteTaskCalled = true;
            return this.OnExecuteTask(context);
        }

        /// <inheritdoc />
        public override Task Kill() => Task.CompletedTask;
    }
}
