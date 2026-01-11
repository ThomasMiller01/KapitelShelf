// <copyright file="HooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Notifications;
using KapitelShelf.Api.Logic.Interfaces;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The hooks logic.
/// </summary>
public class HooksLogic(INotificationsLogic notifications) : IHooksLogic
{
    private readonly INotificationsLogic notifications = notifications;

    /// <inheritdoc/>
    public event Func<Guid, Task>? SessionStart;

    /// <inheritdoc/>
    public async Task DispatchSessionStartAsync(Guid userId)
    {
        var handlers = this.SessionStart;
        if (handlers is null)
        {
            return;
        }

        foreach (Func<Guid, Task> handler in handlers.GetInvocationList().Cast<Func<Guid, Task>>())
        {
            try
            {
                await handler(userId);
            }
            catch (Exception ex)
            {
                _ = this.notifications.AddNotification(
                    "HookDispatchFailed",
                    titleArgs: ["Session Start"],
                    messageArgs: [ex.Message],
                    type: NotificationTypeDto.System,
                    severity: NotificationSeverityDto.Critical,
                    source: "Hook [Session Start]");
            }
        }
    }
}
