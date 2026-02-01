// <copyright file="DynamicSettingsManager.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Ai;
using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Data;
using KapitelShelf.Data.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// The dynamic settings manager.
/// </summary>
public class DynamicSettingsManager(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper) : IDynamicSettingsManager
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    /// <inheritdoc/>
    public async Task InitializeOnStartup()
    {
#pragma warning disable IDE0022 // Use expression body for method
        // cloudstorage
        await this.AddIfNotExists(StaticConstants.DynamicSettingCloudStorageExperimentalBisync, false);

        // ai
        await this.AddIfNotExists(StaticConstants.DynamicSettingAiProvider, AiProvider.None.ToString());
        await this.AddIfNotExists(StaticConstants.DynamicSettingAiProviderConfigured, false);
        await this.AddIfNotExists<List<string>>(StaticConstants.DynamicSettingAiEnabledFeatures, [
            AiFeatures.BookImportMetadataGeneration.ToString(),
        ]);

        await this.AddIfNotExists(StaticConstants.DynamicSettingAiOllamaUrl, "http://host.docker.internal:11434");
        await this.AddIfNotExists(StaticConstants.DynamicSettingAiOllamaModel, "gemma2:9b");
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

        return this.mapper.SettingsModelToSettingsDto<T>(setting);
    }

    /// <inheritdoc/>
    public async Task<SettingsDTO<T>> SetAsync<T>(string key, T value)
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        var setting = await context.Settings
            .FirstOrDefaultAsync(x => x.Key == key);

        if (setting == null)
        {
            throw new KeyNotFoundException($"Setting with key '{key}' not found");
        }

        var valueJson = JsonConvert.SerializeObject(value);

        if (!ValidateValue(setting, valueJson))
        {
            throw new InvalidOperationException(StaticConstants.InvalidSettingValueType);
        }

        setting.Value = valueJson ?? throw new InvalidOperationException(StaticConstants.InvalidSettingValueType);

        await context.SaveChangesAsync();

        return this.mapper.SettingsModelToSettingsDto<T>(setting);
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
            string => SettingsValueType.TString,
            List<string> => SettingsValueType.TListString,
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

        return setting.Type switch
        {
            SettingsValueType.TBoolean => (T)(object)bool.Parse(setting.Value),
            SettingsValueType.TString => (T)(object)(JsonConvert.DeserializeObject<string>(setting.Value) ?? string.Empty),
            SettingsValueType.TListString => (T)(object)(TryDeserializeStringList(setting.Value) ?? []),
            _ => throw new InvalidOperationException("Invalid value type."),
        };
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

        if (value is null or "null")
        {
            return false;
        }

        return setting.Type switch
        {
            SettingsValueType.TBoolean => bool.TryParse(value, out _),
            SettingsValueType.TString => value is not null,
            SettingsValueType.TListString => TryDeserializeStringList(value) is not null,
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
    internal async Task AddIfNotExists<T>(string key, T value)
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
            Value = JsonConvert.SerializeObject(value) ?? throw new InvalidOperationException("Value cannot be converted to string"),
            Type = MapTypeToValueType(value),
        });

        await context.SaveChangesAsync();
    }

    internal static List<string>? TryDeserializeStringList(string value)
    {
        try
        {
            var result = JsonConvert.DeserializeObject<List<string>>(value);
            return result;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
