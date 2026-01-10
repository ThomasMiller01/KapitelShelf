// <copyright file="LocalizationProvider.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Microsoft.Extensions.Localization;

namespace KapitelShelf.Api.Localization;

/// <summary>
/// Provider with common functionality when using localizations.
/// </summary>
/// <typeparam name="T">The localozation resource.</typeparam>
public class LocalizationProvider<T>(IStringLocalizer<T> localizer) : ILocalizationProvider<T>
    where T : class
{
    private readonly IStringLocalizer localizer = localizer;

    /// <inheritdoc/>
    public string Get(string key, params object[] args) => this.localizer[key, args].Value;
}
