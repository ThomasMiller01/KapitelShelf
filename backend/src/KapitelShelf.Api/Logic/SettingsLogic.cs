// <copyright file="SettingsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Resources;
using KapitelShelf.Api.Tasks.Ai;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="SettingsLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
/// <param name="schedulerFactory">The scheduler factory.</param>
/// <param name="settingsManager">The settings manager.</param>
public class SettingsLogic(
    IDbContextFactory<KapitelShelfDBContext> dbContextFactory,
    Mapper mapper,
    ISchedulerFactory schedulerFactory,
    IDynamicSettingsManager settingsManager) : ISettingsLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    private readonly ISchedulerFactory scheduleFactory = schedulerFactory;

    private readonly IDynamicSettingsManager settingsManager = settingsManager;

    private readonly string[] aiConfigureProviderSideEffects = [
        StaticConstants.DynamicSettingAiProvider,
        StaticConstants.DynamicSettingAiOllamaUrl,
        StaticConstants.DynamicSettingAiOllamaModel,
    ];

    /// <inheritdoc/>
    public async Task<List<SettingsDTO<object>>> GetSettingsAsync()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Settings
            .Select(x => this.mapper.SettingsModelToSettingsDto<object>(x))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<SettingsDTO<object>?> UpdateSettingAsync(Guid settingId, object value)
    {
        if (value is null)
        {
            return null;
        }

        await using var context = await this.dbContextFactory.CreateDbContextAsync();

        var setting = await context.Settings
            .FirstOrDefaultAsync(b => b.Id == settingId);

        if (setting is null)
        {
            return null;
        }

        // check, if the passed value can be used for this setting
        if (!DynamicSettingsManager.ValidateValue(setting, value.ToString()))
        {
            throw new InvalidOperationException(StaticConstants.InvalidSettingValueType);
        }

        // patch setting root scalars
        context.Entry(setting).CurrentValues.SetValues(new
        {
            Value = value.ToString() ?? throw new InvalidOperationException(StaticConstants.InvalidSettingValueType),
        });

        // commit
        await context.SaveChangesAsync();

        var settingDto = this.mapper.SettingsModelToSettingsDto<object>(setting);

        await this.SettingUpdateSideEffectsAsync(settingDto);

        return settingDto;
    }

    internal async Task SettingUpdateSideEffectsAsync(SettingsDTO<object> setting)
    {
        // trigger ai provider configuration
        if (this.aiConfigureProviderSideEffects.Contains(setting.Key))
        {
            await this.settingsManager.SetAsync(StaticConstants.DynamicSettingAiProviderConfigured, false);

            var scheduler = await this.scheduleFactory.GetScheduler();
            await ConfigureProvider.Schedule(scheduler);
        }
    }
}
