// <copyright file="IAiManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Ai;
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

    /// <summary>
    /// Check if a feature is currently enabled.
    /// </summary>
    /// <param name="feature">The feature to check.</param>
    /// <returns>True, if the feature is enabled, otherwise false.</returns>
    Task<bool> FeatureEnabled(AiFeatures feature);

    /// <summary>
    /// Make a prompt and get a structured result.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="userPrompt">The user prompt.</param>
    /// <param name="systemPrompt">The system prompt.</param>
    /// <returns>The structured result.</returns>
    Task<T?> GetStructuredResponse<T>(string userPrompt, string? systemPrompt = null)
        where T : class;
}
