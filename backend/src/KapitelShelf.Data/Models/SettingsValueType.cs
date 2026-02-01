// <copyright file="SettingsValueType.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The settings value type.
/// </summary>
public enum SettingsValueType
{
    /// <summary>
    /// The "boolean" type.
    /// </summary>
    TBoolean = 0,

    /// <summary>
    /// The "string" type.
    /// </summary>
    TString = 1,

    /// <summary>
    /// The "List{string}" type.
    /// </summary>
    TListString = 2,
}
