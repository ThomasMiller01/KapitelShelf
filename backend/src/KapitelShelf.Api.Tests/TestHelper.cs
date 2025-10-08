// <copyright file="TestHelper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.Mappings;

#pragma warning disable IDE0022 // Use expression body for method

namespace KapitelShelf.Api.Tests;

/// <summary>
/// Helper class for unit tests.
/// </summary>
public static class Testhelper
{
    /// <summary>
    /// Gets the path to a resource file in the test project.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The file path.</returns>
    public static string GetResourcePath(string fileName)
    {
        return Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", fileName);
    }

    /// <summary>
    /// Creates the application mapper for tests.
    /// </summary>
    /// <returns>The configured mapper.</returns>
    public static Mapper CreateMapper() => new Mapper();

    /// <summary>
    /// Make a string unique.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <returns>The unique string.</returns>
    public static string Unique(this string value)
    {
        return $"{value}_{Guid.NewGuid()}";
    }
}
