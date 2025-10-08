// <copyright file="Mapper.Tags.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Tag;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The tags mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a tag model to a tag dto.
    /// </summary>
    /// <param name="model">The tag model.</param>
    /// <returns>The tag dto.</returns>
    [MapperIgnoreSource(nameof(TagModel.Books))]
    public partial TagDTO TagModelToTagDto(TagModel model);

    /// <summary>
    /// Map a create tag dto to a tag model.
    /// </summary>
    /// <param name="dto">The create tag dto.</param>
    /// <returns>The tag model.</returns>
    [MapperIgnoreTarget(nameof(TagModel.Id))]
    [MapperIgnoreTarget(nameof(TagModel.Books))]
    public partial TagModel CreateTagDtoToTagModel(CreateTagDTO dto);

    /// <summary>
    /// Map a tag dto to a create tag dto.
    /// </summary>
    /// <param name="dto">The tag dto.</param>
    /// <returns>The create tag dto.</returns>
    [MapperIgnoreSource(nameof(TagDTO.Id))]
    public partial CreateTagDTO TagDtoToCreateTagDto(TagDTO dto);
}
