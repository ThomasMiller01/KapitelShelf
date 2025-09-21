// <copyright file="SettingMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Settings;
using KapitelShelf.Api.Logic;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The setting mapping profile.
/// </summary>
public class SettingMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingMappingProfile"/> class.
    /// </summary>
    public SettingMappingProfile()
    {
        this.AddGenericSettingsMap<bool>();

        CreateMap<SettingsValueType, SettingsValueTypeDTO>()
            .ReverseMap();
    }

    private void AddGenericSettingsMap<T>()
    {
        CreateMap<SettingsModel, SettingsDTO<T>>()
            .ConvertUsing((src, _, ctx) =>
            {
                return new SettingsDTO<T>
                {
                    Id = src.Id,
                    Key = src.Key,
                    Value = DynamicSettingsManager.ConvertSettingValue<T>(src),
                };
            });

        CreateMap<SettingsDTO<T>, SettingsModel>()
            .ConvertUsing((src, _, ctx) =>
            {
                return new SettingsModel
                {
                    Id = src.Id,
                    Key = src.Key,
                    Value = src.Value?.ToString() ?? throw new InvalidCastException("Value could not be mapped."),
                    Type = DynamicSettingsManager.MapTypeToValueType(src.Value),
                };
            });
    }
}
