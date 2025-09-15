// <copyright file="UserSettingValueTypeDTO.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Api.DTOs.User;

/// <summary>
/// The user setting value type enum.
/// </summary>
public enum UserSettingValueTypeDTO
{
    /// <summary>
    /// The "string" type.
    /// </summary>
    TString = 0,

    /// <summary>
    /// The "integer" type.
    /// </summary>
    TInteger = 1,

    /// <summary>
    /// The "boolean" type.
    /// </summary>
    TBoolean = 2,
}
