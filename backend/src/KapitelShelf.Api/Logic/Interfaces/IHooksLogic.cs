// <copyright file="IHooksLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The hooks logic interface.
/// </summary>
public interface IHooksLogic
{
    /// <summary>
    /// The session start event, called once per user upon application access.
    /// </summary>
    event Func<Guid, Task>? SessionStart;

    /// <summary>
    /// Executes the session start hook.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>A task.</returns>
    Task DispatchSessionStartAsync(Guid userId);
}
