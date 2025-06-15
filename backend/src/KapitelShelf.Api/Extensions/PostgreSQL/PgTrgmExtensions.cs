// <copyright file="PgTrgmExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Api.Extensions.PostgreSQL;

/// <summary>
/// PostgreSQL Trigram Extension.
/// </summary>
public static class PgTrgmExtensions
{
    /// <summary>
    /// The similarity method of the trigram extension.
    /// </summary>
    /// <param name="a">Param a.</param>
    /// <param name="b">Param b.</param>
    /// <returns>The similarity.</returns>
    /// <exception cref="NotImplementedException">Has to be invoked as a PostgreSQL extension.</exception>
    [DbFunction("similarity", IsBuiltIn = true)]
    public static double Similarity(string a, string b) => throw new NotImplementedException();
}
