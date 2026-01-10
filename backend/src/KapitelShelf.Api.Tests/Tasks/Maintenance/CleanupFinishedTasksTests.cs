// <copyright file="CleanupFinishedTasksTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Tasks;
using KapitelShelf.Api.Tasks.Maintenance;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;
using Quartz.Impl.Matchers;

namespace KapitelShelf.Api.Tests.Tasks.Maintenance;

/// <summary>
/// Unit tests for the CleanupFinishedTask class.
/// </summary>
[TestFixture]
public class CleanupFinishedTasksTests
{
    private ITaskRuntimeDataStore dataStore;
    private ILogger<TaskBase> logger;
    private INotificationsLogic notificationsLogic;
    private ISchedulerFactory schedulerFactory;
    private IScheduler scheduler;
    private CleanupFinishedTasks testee;
    private IJobExecutionContext context;

    /// <summary>
    /// Sets up the testing class.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.dataStore = Substitute.For<ITaskRuntimeDataStore>();
        this.logger = Substitute.For<ILogger<TaskBase>>();
        this.notificationsLogic = Substitute.For<INotificationsLogic>();
        this.schedulerFactory = Substitute.For<ISchedulerFactory>();
        this.scheduler = Substitute.For<IScheduler>();
        this.context = Substitute.For<IJobExecutionContext>();

        // setupo job key
        var jobKey = new JobKey("UnitTestJob", "TestGroup");
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(jobKey);
        this.context.JobDetail.Returns(jobDetail);

        this.testee = new CleanupFinishedTasks(this.dataStore, this.logger, this.notificationsLogic, this.schedulerFactory);
    }

    /// <summary>
    /// Tests ExecuteTask deletes jobs with all complete triggers.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_DeletesJobsWithAllCompleteTriggers()
    {
        // Setup
        // - One group with one job, all triggers complete
        var jobKey = new JobKey("Job1", "Group1");
        var triggerKey = new TriggerKey("Trigger1", "Group1");
        var trigger = Substitute.For<ITrigger>();
        trigger.Key.Returns(triggerKey);

        this.schedulerFactory.GetScheduler().Returns(Task.FromResult(this.scheduler));
        this.scheduler.GetTriggerGroupNames().Returns(Task.FromResult((IReadOnlyCollection<string>)["Group1"]));
        this.scheduler.GetJobKeys(Arg.Any<GroupMatcher<JobKey>>()).Returns(Task.FromResult((IReadOnlyCollection<JobKey>)[jobKey]));
        this.scheduler.GetTriggersOfJob(jobKey).Returns(Task.FromResult((IReadOnlyCollection<ITrigger>)[trigger]));
        this.scheduler.GetTriggerState(triggerKey).Returns(Task.FromResult(TriggerState.Complete));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.scheduler.Received().DeleteJob(jobKey);
        this.dataStore.Received().SetProgress(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
    }

    /// <summary>
    /// Tests ExecuteTask resets error triggers and does not delete if any are not completed.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_ResetsErrorTriggersAndDoesNotDeleteIfAnyNotComplete()
    {
        // Setup
        // - One group, one job, one trigger in Error, one in Normal
        var jobKey = new JobKey("Job2", "Group1");
        var triggerKey1 = new TriggerKey("Trigger1", "Group1");
        var triggerKey2 = new TriggerKey("Trigger2", "Group1");

        var trigger1 = Substitute.For<ITrigger>();
        var trigger2 = Substitute.For<ITrigger>();
        trigger1.Key.Returns(triggerKey1);
        trigger2.Key.Returns(triggerKey2);

        this.schedulerFactory.GetScheduler().Returns(Task.FromResult(this.scheduler));
        this.scheduler.GetTriggerGroupNames().Returns(Task.FromResult((IReadOnlyCollection<string>)["Group1"]));
        this.scheduler.GetJobKeys(Arg.Any<GroupMatcher<JobKey>>()).Returns(Task.FromResult((IReadOnlyCollection<JobKey>)[jobKey]));
        this.scheduler.GetTriggersOfJob(jobKey).Returns(Task.FromResult((IReadOnlyCollection<ITrigger>)[trigger1, trigger2]));
        this.scheduler.GetTriggerState(triggerKey1).Returns(Task.FromResult(TriggerState.Error));
        this.scheduler.GetTriggerState(triggerKey2).Returns(Task.FromResult(TriggerState.Normal));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.scheduler.Received().ResetTriggerFromErrorState(triggerKey1);
        await this.scheduler.DidNotReceive().DeleteJob(jobKey);
        this.dataStore.Received().SetProgress(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
    }

    /// <summary>
    /// Tests ExecuteTask skips delete if not all are complete.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task ExecuteTask_SkipsDeleteIfNotAllComplete()
    {
        // Setup
        // - One group, one job, trigger is not complete
        var jobKey = new JobKey("Job3", "Group1");
        var triggerKey = new TriggerKey("Trigger3", "Group1");
        var trigger = Substitute.For<ITrigger>();
        trigger.Key.Returns(triggerKey);

        this.schedulerFactory.GetScheduler().Returns(Task.FromResult(this.scheduler));
        this.scheduler.GetTriggerGroupNames().Returns(Task.FromResult((IReadOnlyCollection<string>)["Group1"]));
        this.scheduler.GetJobKeys(Arg.Any<GroupMatcher<JobKey>>()).Returns(Task.FromResult((IReadOnlyCollection<JobKey>)[jobKey]));
        this.scheduler.GetTriggersOfJob(jobKey).Returns(Task.FromResult((IReadOnlyCollection<ITrigger>)[trigger]));
        this.scheduler.GetTriggerState(triggerKey).Returns(Task.FromResult(TriggerState.Normal));

        // Execute
        await this.testee.ExecuteTask(this.context);

        // Assert
        await this.scheduler.DidNotReceive().DeleteJob(jobKey);
    }

    /// <summary>
    /// Tests ExecuteTask handles when there are no jobs or groups.
    /// </summary>
    [Test]
    public void ExecuteTask_HandlesNoJobsOrGroups()
    {
        // Setup
        // - No groups/jobs
        this.schedulerFactory.GetScheduler().Returns(Task.FromResult(this.scheduler));
        this.scheduler.GetTriggerGroupNames().Returns(Task.FromResult((IReadOnlyCollection<string>)[]));

        // Execute and Assert
        Assert.DoesNotThrowAsync(async () => await this.testee.ExecuteTask(this.context));
    }

    /// <summary>
    /// Tests that kill completes when called.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Kill_DoesNothing() => await this.testee.Kill();

    /// <summary>
    /// Tests Schedule creates job and trigger.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task Schedule_CreatesJobAndTrigger()
    {
        // Setup
        var scheduler = Substitute.For<IScheduler>();
        var called = false;
        scheduler.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<IReadOnlyCollection<ITrigger>>(), true)
            .Returns(callInfo =>
            {
                called = true;
                return Task.CompletedTask;
            });

        // Execute
        var key = await CleanupFinishedTasks.Schedule(scheduler);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(key, Is.Not.Null.And.Not.Empty);
            Assert.That(called, Is.True);
        });
    }
}
