// <copyright file="TestHelper.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.Mappings;

namespace KapitelShelf.Api.Tests;

/// <summary>
/// Helper class for unit tests.
/// </summary>
public static class Testhelper
{
    /// <summary>
    /// Creates an auto mapper.
    /// </summary>
    /// <returns>The auto mapper.</returns>
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuthorMappingProfile>();
            cfg.AddProfile<BookMappingProfile>();
            cfg.AddProfile<CategoryMappingProfile>();
            cfg.AddProfile<FileInfoMappingProfile>();
            cfg.AddProfile<LocationMappingProfile>();
            cfg.AddProfile<SeriesMappingProfile>();
            cfg.AddProfile<TagMappingProfile>();
        });
        return config.CreateMapper();
    }
}
