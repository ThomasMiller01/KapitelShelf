// <copyright file="JsonExtensions.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Text.Json;

namespace KapitelShelf.Api.Extensions;

/// <summary>
/// Extensions for working with JSON data.
/// </summary>
public static class OpenLibraryJsonExtensions
{
    /// <summary>
    /// Returns a property as string.
    /// </summary>
    /// <param name="elem">The JSON element to check.</param>
    /// <param name="property"> The property name to look for.</param>
    /// <returns>The property as string if exists, otherwise null.</returns>
    public static string? GetPropertyOrDefault(this JsonElement elem, string property)
    {
        if (elem.TryGetProperty(property, out var val))
        {
            return val.ValueKind switch
            {
                JsonValueKind.String => val.GetString(),
                JsonValueKind.Number => val.GetRawText(),
                _ => null
            };
        }

        return null;
    }

    /// <summary>
    /// Returns an array property as list of strings.
    /// </summary>
    /// <param name="elem">The JSON element to check.</param>
    /// <param name="property"> The property name to look for.</param>
    /// <returns>A list of strings if the property exists and is an array, otherwise an empty list.</returns>
    public static List<string> GetArrayOrDefault(this JsonElement elem, string property)
    {
        if (elem.TryGetProperty(property, out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            return arr.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        return [];
    }

    /// <summary>
    /// Gets the first string of an array property.
    /// </summary>
    /// <param name="elem">The JSON element to check.</param>
    /// <param name="property"> The property name to look for.</param>
    /// <returns>The first string in the array if it exists, otherwise null.</returns>
    public static string? GetFirstOrDefault(this JsonElement elem, string property)
    {
        if (elem.TryGetProperty(property, out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in arr.EnumerateArray())
            {
                if (entry.ValueKind == JsonValueKind.String)
                {
                    return entry.GetString();
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets an int value from a property.
    /// </summary>
    /// <param name="elem">The JSON element to check.</param>
    /// <param name="property"> The property name to look for.</param>
    /// <returns>An int if the property exists and is a number, otherwise null.</returns>
    public static int? GetIntOrNull(this JsonElement elem, string property)
    {
        if (elem.TryGetProperty(property, out var val) && val.ValueKind == JsonValueKind.Number)
        {
            if (val.TryGetInt32(out var result))
            {
                return result;
            }
        }

        return null;
    }
}
