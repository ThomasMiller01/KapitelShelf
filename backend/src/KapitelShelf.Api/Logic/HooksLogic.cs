// <copyright file="HooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Logic.Interfaces;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The hooks logic.
/// </summary>
public class HooksLogic : IHooksLogic
{
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
            await handler(userId);
        }
    }
}
