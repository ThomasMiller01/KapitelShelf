// <copyright file="ILocalizationProvider.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.Localization;

/// <summary>
/// The LocalizationProvider interface.
/// </summary>
/// <typeparam name="T">The localization resource.</typeparam>
public interface ILocalizationProvider<T>
    where T : class
{
    /// <summary>
    /// Get a localized string template.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="args">Arguments for positional formatting.</param>
    /// <returns>The string template.</returns>
#pragma warning disable CA1716 // Identifiers should not match keywords
    string Get(string key, params object[] args);
#pragma warning restore CA1716 // Identifiers should not match keywords
}
