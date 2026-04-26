// <copyright file="Mapper.Reader.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Globalization;
using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Api.DTOs.Book;
using KapitelShelf.Api.DTOs.Category;
using KapitelShelf.Api.DTOs.MetadataScraper;
using KapitelShelf.Api.DTOs.Reader;
using KapitelShelf.Api.DTOs.Series;
using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The books mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a reading book model to a reading book dto.
    /// </summary>
    /// <param name="model">The reading book model.</param>
    /// <returns>The reading book dto.</returns>
    [MapperIgnoreSource(nameof(ReadingBooksModel.Id))]
    [MapperIgnoreSource(nameof(ReadingBooksModel.Book))]
    [MapperIgnoreSource(nameof(ReadingBooksModel.User))]
    [MapperIgnoreSource(nameof(ReadingBooksModel.LastReadAt))]
    public partial ReadingBookDTO ReadingBookModelToReadingBookDto(ReadingBooksModel model);
}
