// <copyright file="HooksLogicTests.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic;
using KapitelShelf.Api.Logic.Interfaces;
using NSubstitute;

namespace KapitelShelf.Api.Tests.Logic;

/// <summary>
/// Unit tests for the <see cref="HooksLogic"/> class.
/// </summary>
[TestFixture]
public class HooksLogicTests
{
    private INotificationsLogic notifications;
    private HooksLogic testee;

    /// <summary>
    /// Sets up the testee before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this.notifications = Substitute.For<INotificationsLogic>();
        this.testee = new HooksLogic(this.notifications);
    }

    /// <summary>
    /// Tests DispatchSessionStartAsync does nothing when no handlers are registered.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DispatchSessionStartAsync_DoesNothing_WhenNoHandlersRegistered()
    {
        // Setup
        var userId = Guid.NewGuid();

        // Execute
        await this.testee.DispatchSessionStartAsync(userId);

        // Assert
        _ = this.notifications.DidNotReceiveWithAnyArgs().AddNotification(
            default!,
            titleArgs: default!,
            messageArgs: default!,
            type: default,
            severity: default,
            source: default!,
            userId: default);
    }

    /// <summary>
    /// Tests DispatchSessionStartAsync calls a single registered handler and does not notify.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DispatchSessionStartAsync_CallsHandler_WhenSingleHandlerRegistered()
    {
        // Setup
        var userId = Guid.NewGuid();
        var calledUserId = Guid.Empty;

        this.testee.SessionStart += id =>
        {
            calledUserId = id;
            return Task.CompletedTask;
        };

        // Execute
        await this.testee.DispatchSessionStartAsync(userId);

        // Assert
        Assert.That(calledUserId, Is.EqualTo(userId));

        _ = this.notifications.DidNotReceiveWithAnyArgs().AddNotification(
            default!,
            titleArgs: default!,
            messageArgs: default!,
            type: default,
            severity: default,
            source: default!,
            userId: default);
    }

    /// <summary>
    /// Tests DispatchSessionStartAsync calls all registered handlers.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DispatchSessionStartAsync_CallsAllHandlers_WhenMultipleHandlersRegistered()
    {
        // Setup
        var userId = Guid.NewGuid();
        var callCount = 0;

        this.testee.SessionStart += _ =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        this.testee.SessionStart += _ =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        // Execute
        await this.testee.DispatchSessionStartAsync(userId);

        // Assert
        Assert.That(callCount, Is.EqualTo(2));

        _ = this.notifications.DidNotReceiveWithAnyArgs().AddNotification(
            default!,
            titleArgs: default!,
            messageArgs: default!,
            type: default,
            severity: default,
            source: default!,
            userId: default);
    }

    /// <summary>
    /// Tests DispatchSessionStartAsync adds a notification when a handler throws and continues with next handlers.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DispatchSessionStartAsync_AddsNotification_WhenHandlerThrows_AndContinues()
    {
        // Setup
        var userId = Guid.NewGuid();
        var callCount = 0;

        this.testee.SessionStart += _ => throw new InvalidOperationException("fail");

        this.testee.SessionStart += _ =>
        {
            callCount++;
            return Task.CompletedTask;
        };

        // Execute
        await this.testee.DispatchSessionStartAsync(userId);

        // Assert
        Assert.That(callCount, Is.EqualTo(1));

        _ = this.notifications.Received(1).AddNotification(
            "HookDispatchFailed",
            titleArgs: Arg.Any<object[]>(),
            messageArgs: Arg.Any<object[]>(),
            type: NotificationTypeDto.System,
            severity: NotificationSeverityDto.Critical,
            source: "Hook [Session Start]",
            userId: null);
    }

    /// <summary>
    /// Tests that unsubscribed handlers are not called.
    /// </summary>
    /// <returns>A task.</returns>
    [Test]
    public async Task DispatchSessionStartAsync_DoesNotCallHandler_WhenUnsubscribed()
    {
        // Setup
        var userId = Guid.NewGuid();
        var callCount = 0;

        Task Handler(Guid _)
        {
            callCount++;
            return Task.CompletedTask;
        }

        this.testee.SessionStart += Handler;
        this.testee.SessionStart -= Handler;

        // Execute
        await this.testee.DispatchSessionStartAsync(userId);

        // Assert
        Assert.That(callCount, Is.EqualTo(0));
    }
}
