// <copyright file="SettingsLogic.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic.Interfaces;
using KapitelShelf.Api.Mappings;
using KapitelShelf.Api.Settings;
using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Logic;

/// <summary>
/// Initializes a new instance of the <see cref="SettingsLogic"/> class.
/// </summary>
/// <param name="dbContextFactory">The dbContext factory.</param>
/// <param name="mapper">The mapper.</param>
public class SettingsLogic(IDbContextFactory<KapitelShelfDBContext> dbContextFactory, Mapper mapper) : ISettingsLogic
{
    private readonly IDbContextFactory<KapitelShelfDBContext> dbContextFactory = dbContextFactory;

    private readonly Mapper mapper = mapper;

    /// <inheritdoc/>
    public async Task<List<SettingsDTO<object>>> GetSettingsAsync()
    {
        using var context = await this.dbContextFactory.CreateDbContextAsync();

        return await context.Settings
            .Select(x => this.mapper.SettingsModelToSettingsDtoObject(x))
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

        return this.mapper.SettingsModelToSettingsDtoObject(setting);
    }
}
