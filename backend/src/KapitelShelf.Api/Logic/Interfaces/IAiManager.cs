// <copyright file="IAiManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Microsoft.Extensions.AI;

namespace KapitelShelf.Api.Logic.Interfaces;

/// <summary>
/// The interface for the Ai manager.
/// </summary>
public interface IAiManager
{
    /// <summary>
    /// Gets a chat client based on the current settings.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The selected chat client.</returns>
    Task<IChatClient?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Configure the currently selected ai provider.
    /// </summary>
    /// <param name="progress">The current progress.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task.</returns>
    Task ConfigureCurrentProvider(IProgress<int>? progress = null, CancellationToken cancellationToken = default);
}
