// <copyright file="Mapper.Settings.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The settings mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a settings model to a settings dto object.
    /// </summary>
    /// <param name="model">The settings model.</param>
    /// <typeparam name="T">The setting value type.</typeparam>
    /// <returns>The settings dto object.</returns>
    public SettingsDTO<T> SettingsModelToSettingsDto<T>(SettingsModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new SettingsDTO<T>
        {
            Id = model.Id,
            Key = model.Key,
            Value = DynamicSettingsManager.ConvertSettingValue<T>(model),
        };
    }

    /// <summary>
    /// Map a settings dto object to a settings model.
    /// </summary>
    /// <param name="dto">The settings dto object.</param>
    /// <typeparam name="T">The setting value type.</typeparam>
    /// <returns>The settings model.</returns>
    /// <exception cref="InvalidCastException">If the dto value is null.</exception>
    public SettingsModel SettingsDtoToSettingsModel<T>(SettingsDTO<T> dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new SettingsModel
        {
            Id = dto.Id,
            Key = dto.Key,
            Value = dto.Value?.ToString() ?? throw new InvalidCastException("Value could not be mapped."),
            Type = DynamicSettingsManager.MapTypeToValueType(dto.Value),
        };
    }

    /// <summary>
    /// Map a settings value type to a settings value type dto.
    /// </summary>
    /// <param name="type">The settings value type.</param>
    /// <returns>The settings value type dto.</returns>
    public SettingsValueTypeDTO SettingsValueTypeToSettingsValueTypeDto(SettingsValueType type) => Enum.Parse<SettingsValueTypeDTO>(type.ToString());

    /// <summary>
    /// Map a settings value type dto to a settings value type.
    /// </summary>
    /// <param name="dto">The settings value type dto.</param>
    /// <returns>The settings value type.</returns>
    public SettingsValueType SettingsValueTypeDtoToSettingsValueType(SettingsValueTypeDTO dto) => Enum.Parse<SettingsValueType>(dto.ToString());
}
