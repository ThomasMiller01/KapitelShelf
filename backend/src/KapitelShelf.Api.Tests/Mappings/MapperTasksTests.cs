// <copyright file="MapperTasksTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tasks;
using KapitelShelf.Api.Mappings;
using Quartz;

namespace KapitelShelf.Api.Tests.Mappings;

/// <summary>
/// Unit tests for the tasks mapper.
/// </summary>
[TestFixture]
public class MapperTasksTests
{
    private Mapper testee;

    /// <summary>
    /// Sets up the mapper instance before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // setup
#pragma warning disable IDE0022 // Use expression body for method
        this.testee = new Mapper();
#pragma warning restore IDE0022 // Use expression body for method
    }

    /// <summary>
    /// Tests that TriggerStateToTaskState maps known values correctly.
    /// </summary>
    /// <param name="state">The trigger state input.</param>
    /// <param name="expected">The expected task state output.</param>
    [TestCase(TriggerState.Normal, TaskState.Scheduled)]
    [TestCase(TriggerState.Paused, TaskState.Scheduled)]
    [TestCase(TriggerState.Blocked, TaskState.Scheduled)]
    [TestCase(TriggerState.Complete, TaskState.Finished)]
    [TestCase(TriggerState.Error, TaskState.Finished)]
    [TestCase(TriggerState.None, TaskState.Finished)]
    [TestCase((TriggerState)999, TaskState.Scheduled)]
    public void TriggerStateToTaskState_MapsCorrectly(TriggerState state, TaskState expected)
    {
        // execute
        var result = this.testee.TriggerStateToTaskState(state);

        // assert
        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// Tests that TriggerStateToFinishedReason maps known values correctly.
    /// </summary>
    /// <param name="state">The trigger state input.</param>
    /// <param name="expected">The expected finished reason output.</param>
    [TestCase(TriggerState.Complete, FinishedReason.Completed)]
    [TestCase(TriggerState.Error, FinishedReason.Error)]
    [TestCase(TriggerState.None, FinishedReason.Completed)]
    [TestCase(TriggerState.Normal, null)]
    [TestCase(TriggerState.Paused, null)]
    [TestCase(TriggerState.Blocked, null)]
    [TestCase((TriggerState)999, null)]
    public void TriggerStateToFinishedReason_MapsCorrectly(TriggerState state, FinishedReason? expected)
    {
        // execute
        var result = this.testee.TriggerStateToFinishedReason(state);

        // assert
        Assert.That(result, Is.EqualTo(expected));
    }
}
