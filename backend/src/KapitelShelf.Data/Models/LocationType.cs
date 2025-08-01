﻿// <copyright file="LocationType.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

namespace KapitelShelf.Data.Models;

/// <summary>
/// The location type enum.
/// </summary>
public enum LocationType
{
    /// <summary>
    /// A physical book.
    /// </summary>
    Physical,

    /// <summary>
    /// Hosted in KapiteShelf.
    /// </summary>
    KapitelShelf,

    /// <summary>
    /// A book from kindle.
    /// </summary>
    Kindle,

    /// <summary>
    /// A book from skoobe.
    /// </summary>
    Skoobe,

    /// <summary>
    /// A book from onleihe.
    /// </summary>
    Onleihe,

    /// <summary>
    /// A book in a library.
    /// </summary>
    Library,
}
