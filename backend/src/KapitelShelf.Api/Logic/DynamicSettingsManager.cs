﻿// <copyright file="DynamicSettingsManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The dynamic settings manager.
/// </summary>
public class DynamicSettingsManager(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, IMapper mapper) : IDynamicSettingsManager
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly IMapper mapper = mapper;

    /// <inheritdoc/>
    public async Task InitializeOnStartup()
    {
#pragma warning disable IDE0022 // Use expression body for method
        await this.AddIfNotExists(StaticConstants.DynamicSettingCloudStorageExperimentalBisync, false);
#pragma warning restore IDE0022 // Use expression body for method
    }

    /// <inheritdoc/>
    public async Task<SettingsDTO<T>> GetAsync<T>(string key)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var setting = await context.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key);

        if (setting == null)
        {
            throw new KeyNotFoundException($"Setting with key '{key}' not found");
        }

        return this.mapper.Map<SettingsDTO<T>>(setting);
    }

    /// <summary>
    /// Map a C# type to a database setting value type.
    /// </summary>
    /// <typeparam name="T">The C# type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The setting value type.</returns>
    /// <exception cref="InvalidOperationException">Raised, if the type of the value is not allowed.</exception>
    public static SettingsValueType MapTypeToValueType<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            bool => SettingsValueType.TBoolean,
            _ => throw new InvalidOperationException("Invalid value type."),
        };
    }

    /// <summary>
    /// Convert a setting value to its native type.
    /// </summary>
    /// <typeparam name="T">The native type.</typeparam>
    /// <param name="setting">The setting.</param>
    /// <returns>The value in its native type.</returns>
    /// <exception cref="InvalidOperationException">Invalid value type.</exception>
    public static T ConvertSettingValue<T>(SettingsModel setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        switch (setting.Type)
        {
            case SettingsValueType.TBoolean:
                var result = bool.Parse(setting.Value);
                return (T)(object)result;
            default:
                throw new InvalidOperationException("Invalid value type.");
        }
    }

    /// <summary>
    /// Validate a value for a specific setting, requiring a type match.
    /// </summary>
    /// <param name="setting">The setting to check against.</param>
    /// <param name="value">The value to check.</param>
    /// <returns>True, if the value is the same type, as the setting value type.</returns>
    public static bool ValidateValue(SettingsModel setting, string? value)
    {
        ArgumentNullException.ThrowIfNull(setting);

        if (value == null)
        {
            return false;
        }

        return setting.Type switch
        {
            SettingsValueType.TBoolean => bool.TryParse(value, out _),
            _ => false,
        };
    }

    /// <summary>
    /// Add a new setting, if it does not exist yet.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>A task.</returns>
    private async Task AddIfNotExists<T>(string key, T value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var setting = await context.Settings
            .FirstOrDefaultAsync(x => x.Key == key);

        if (setting is not null)
        {
            // setting already exists
            return;
        }

        // add new setting
        await context.Settings.AddAsync(new SettingsModel
        {
            Key = key,
            Value = value.ToString() ?? throw new InvalidOperationException("Value cannot be converted to string"),
            Type = MapTypeToValueType(value),
        });

        await context.SaveChangesAsync();
    }
}
