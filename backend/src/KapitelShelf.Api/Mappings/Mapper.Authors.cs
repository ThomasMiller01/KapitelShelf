// <copyright file="Mapper.Authors.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using KapitelShelf.Api.DTOs.Author;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

[assembly: InternalsVisibleTo("KapitelShelf.Api.Tests")]

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The authors mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map an author model to an author dto, applying additional manual field logic.
    /// </summary>
    /// <param name="model">The author model.</param>
    /// <returns>The mapped author dto.</returns>
    [UserMapping(Default = true)]
    public AuthorDTO AuthorModelToAuthorDto(AuthorModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var dto = this.AuthorModelToAuthorDtoCore(model);

        dto.TotalBooks = model.Books.Count;

        return dto;
    }

    /// <summary>
    /// Map a author model to a author dto.
    /// </summary>
    /// <param name="model">The author model.</param>
    /// <returns>The author dto.</returns>
    [MapperIgnoreSource(nameof(AuthorModel.Books))]
    [MapperIgnoreSource(nameof(AuthorModel.CreatedAt))]
    [MapperIgnoreSource(nameof(AuthorModel.UpdatedAt))]
    [MapperIgnoreTarget(nameof(AuthorDTO.TotalBooks))]
    public partial AuthorDTO AuthorModelToAuthorDtoCore(AuthorModel model);

    /// <summary>
    /// Map a create author dto to a author model.
    /// </summary>
    /// <param name="dto">The create author dto.</param>
    /// <returns>The author model.</returns>
    [MapperIgnoreTarget(nameof(AuthorModel.Id))]
    [MapperIgnoreTarget(nameof(AuthorModel.Books))]
    [MapperIgnoreTarget(nameof(AuthorModel.CreatedAt))]
    [MapperIgnoreTarget(nameof(AuthorModel.UpdatedAt))]
    public partial AuthorModel CreateAuthorDtoToAuthorModel(CreateAuthorDTO dto);

    /// <summary>
    /// Map a author dto to a create author dto.
    /// </summary>
    /// <param name="dto">The author dto.</param>
    /// <returns>The create author dto.</returns>
    [MapperIgnoreSource(nameof(AuthorDTO.Id))]
    [MapperIgnoreSource(nameof(AuthorDTO.TotalBooks))]
    public partial CreateAuthorDTO AuthorDtoToCreateAuthorDto(AuthorDTO dto);

    internal static (string FirstName, string LastName) SplitName(string value)
    {
        var parts = value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => (string.Empty, string.Empty),
            1 => (string.Empty, parts[0]),
            _ => (parts[0], parts[1]),
        };
    }
}
