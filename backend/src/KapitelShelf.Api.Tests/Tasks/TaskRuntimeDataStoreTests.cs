// <copyright file="TaskRuntimeDataStoreTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Tasks;

namespace KapitelShelf.Api.Tests.Tasks;

/// <summary>
/// UnitTests for the TaskRuntimeDataStore class.
/// </summary>
[TestFixture]
public class TaskRuntimeDataStoreTests
{
    private TaskRuntimeDataStore testee;

    /// <summary>
    /// Sets up a new TaskRuntimeDataStore before each test.
    /// </summary>
    [SetUp]
    public void SetUp() => this.testee = new TaskRuntimeDataStore();

    /// <summary>
    /// Tests SetProgress and GetProgress store and retrieve progress percentage.
    /// </summary>
    [Test]
    public void SetProgress_StoresAndRetrievesProgress()
    {
        // Setup
        var jobKey = "job-1";

        // Execute
        this.testee.SetProgress(jobKey, 42);

        // Assert
        Assert.That(this.testee.GetProgress(jobKey), Is.EqualTo(42));
    }

    /// <summary>
    /// Tests SetProgress with current and total computes correct percentage.
    /// </summary>
    [Test]
    public void SetProgress_WithCurrentAndTotal_ComputesPercentage()
    {
        // Setup
        var jobKey = "job-2";
        int current = 2, total = 5;

        // Execute
        this.testee.SetProgress(jobKey, current, total);

        // Assert
        Assert.That(this.testee.GetProgress(jobKey), Is.EqualTo(40));
    }

    /// <summary>
    /// setup: create a new TaskRuntimeDataStore.
    /// execute: call SetMultiItemProgress with currentIndex=0, totalItems=3, itemPercentage=0.
    /// assert: progress should be 0.
    /// </summary>
    [Test]
    public void SetMultiItemProgress_FirstItemZeroPercent_SetsProgressToZero()
    {
        // setup
        var store = new TaskRuntimeDataStore();
        var jobKey = "job1";

        // execute
        store.SetProgress(jobKey, 0, 3, 0);

        // assert
        var progress = store.GetProgress(jobKey);
        Assert.That(progress, Is.Not.Null);
        Assert.That(progress, Is.EqualTo(0));
    }

    /// <summary>
    /// setup: create a new TaskRuntimeDataStore.
    /// execute: call SetMultiItemProgress with currentIndex=1, totalItems=4, itemPercentage=50.
    /// assert: progress should be 37 (rounded from 37.5).
    /// </summary>
    [Test]
    public void SetMultiItemProgress_SecondItemHalfway_SetsCorrectProgress()
    {
        // setup
        var store = new TaskRuntimeDataStore();
        var jobKey = "job2";

        // execute
        store.SetProgress(jobKey, 1, 4, 50);

        // assert
        // (1 * 100 + 50) / 4 = 37.5 => 37
        var progress = store.GetProgress(jobKey);
        Assert.That(progress, Is.Not.Null);
        Assert.That(progress, Is.EqualTo(37));
    }

    /// <summary>
    /// setup: create a new TaskRuntimeDataStore.
    /// execute: call SetMultiItemProgress with currentIndex=2, totalItems=3, itemPercentage=100.
    /// assert: progress should be 100.
    /// </summary>
    [Test]
    public void SetMultiItemProgress_LastItemFull_SetsProgressToHundred()
    {
        // setup
        var store = new TaskRuntimeDataStore();
        var jobKey = "job3";

        // execute
        store.SetProgress(jobKey, 2, 3, 100);

        // assert
        var progress = store.GetProgress(jobKey);
        Assert.That(progress, Is.Not.Null);
        Assert.That(progress, Is.EqualTo(100));
    }

    /// <summary>
    /// setup: create a new TaskRuntimeDataStore.
    /// execute: call SetMultiItemProgress with totalItems=0.
    /// assert: progress should be 0 (should not divide by zero).
    /// </summary>
    [Test]
    public void SetMultiItemProgress_ZeroTotalItems_SetsProgressToZero()
    {
        // setup
        var store = new TaskRuntimeDataStore();
        var jobKey = "job4";

        // execute
        store.SetProgress(jobKey, 0, 0, 100);

        // assert
        var progress = store.GetProgress(jobKey);
        Assert.That(progress, Is.Not.Null);
        Assert.That(progress, Is.EqualTo(0));
    }

    /// <summary>
    /// Tests GetProgress returns null if no progress was set.
    /// </summary>
    [Test]
    public void GetProgress_ReturnsNull_IfNotSet()
    {
        // Setup
        var jobKey = "unknown";

        // Execute
        var progress = this.testee.GetProgress(jobKey);

        // Assert
        Assert.That(progress, Is.Null);
    }

    /// <summary>
    /// Tests SetMessage and GetMessage store and retrieve messages.
    /// </summary>
    [Test]
    public void SetMessage_StoresAndRetrievesMessage()
    {
        // Setup
        var jobKey = "job-msg";
        var message = "Test Message";

        // Execute
        this.testee.SetMessage(jobKey, message);

        // Assert
        Assert.That(this.testee.GetMessage(jobKey), Is.EqualTo(message));
    }

    /// <summary>
    /// Tests GetMessage returns null if no message was set.
    /// </summary>
    [Test]
    public void GetMessage_ReturnsNull_IfNotSet()
    {
        // Setup
        var jobKey = "job-no-msg";

        // Execute
        var msg = this.testee.GetMessage(jobKey);

        // Assert
        Assert.That(msg, Is.Null);
    }

    /// <summary>
    /// Tests ClearData removes progress and message for a job.
    /// </summary>
    [Test]
    public void ClearData_RemovesProgressAndMessage()
    {
        // Setup
        var jobKey = "job-clear";
        this.testee.SetProgress(jobKey, 55);
        this.testee.SetMessage(jobKey, "To be removed");

        // Execute
        this.testee.ClearData(jobKey);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(this.testee.GetProgress(jobKey), Is.Null);
            Assert.That(this.testee.GetMessage(jobKey), Is.Null);
        });
    }
}
