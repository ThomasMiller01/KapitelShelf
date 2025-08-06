// <copyright file="WaitForTaskListenerTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Tasks;
using NSubstitute;
using Quartz;

namespace KapitelShelf.Api.Tests.Tasks;

/// <summary>
/// Unit tests for WaitForTaskListener.
/// </summary>
[TestFixture]
public class WaitForTaskListenerTests
{
    private readonly string jobKeyName = "jobName";
    private readonly string jobKeyGroup = "jobGroup";
    private WaitForTaskListener testee;

    /// <summary>
    /// Tests SetUp configures a valid test instance.
    /// </summary>
    [SetUp]
    public void SetUp() => this.testee = new WaitForTaskListener(this.jobKeyGroup + "." + this.jobKeyName);

    /// <summary>
    /// Tests Name property returns PublicName.
    /// </summary>
    [Test]
    public void Name_ReturnsPublicName()
    {
        // Execute
        var name = this.testee.Name;

        // Assert
        Assert.That(name, Is.EqualTo(WaitForTaskListener.PublicName));
    }

    /// <summary>
    /// Tests JobToBeExecuted and JobExecutionVetoed do nothing.
    /// </summary>
    [Test]
    public void JobToBeExecuted_And_JobExecutionVetoed_DoNothing()
    {
        // Setup
        var context = Substitute.For<IJobExecutionContext>();

        // Execute and Assert
        Assert.DoesNotThrowAsync(async () => await this.testee.JobToBeExecuted(context));
        Assert.DoesNotThrowAsync(async () => await this.testee.JobExecutionVetoed(context));
    }

    /// <summary>
    /// Tests WaitAsync returns true when job finished without exception.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task WaitAsync_ReturnsTrue_WhenJobFinishesSuccessfully()
    {
        // Setup
        var context = Substitute.For<IJobExecutionContext>();
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(new JobKey(this.jobKeyName, this.jobKeyGroup));
        context.JobDetail.Returns(jobDetail);

        // Execute
        await this.testee.JobWasExecuted(context, null);
        var result = await this.testee.WaitAsync(5);

        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests WaitAsync returns false when job finished with exception.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task WaitAsync_ReturnsFalse_WhenJobFails()
    {
        // Setup
        var context = Substitute.For<IJobExecutionContext>();
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(new JobKey(this.jobKeyName, this.jobKeyGroup));
        context.JobDetail.Returns(jobDetail);

        var jobEx = new JobExecutionException("fail!");

        // Execute
        await this.testee.JobWasExecuted(context, jobEx);
        var result = await this.testee.WaitAsync(5);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests JobWasExecuted ignores jobs with other keys.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task JobWasExecuted_DoesNotCompleteTask_IfKeyDoesNotMatch()
    {
        // Setup
        var context = Substitute.For<IJobExecutionContext>();
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.Key.Returns(new JobKey("OtherJobName", "OtherJobGroup"));
        context.JobDetail.Returns(jobDetail);

        // Execute
        await this.testee.JobWasExecuted(context, null);
        var waitTask = this.testee.WaitAsync(5);

        // Assert
        Assert.That(waitTask.IsCompleted, Is.False);
    }

    /// <summary>
    /// Tests JobWasExecuted throws ArgumentNullException if context is null.
    /// </summary>
    [Test]
    public void JobWasExecuted_ThrowsIfContextNull() => Assert.ThrowsAsync<ArgumentNullException>(async () => await this.testee.JobWasExecuted(null!, null));
}
