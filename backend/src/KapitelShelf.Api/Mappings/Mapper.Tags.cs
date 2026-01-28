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
    /// Map an category model to an category dto, applying additional manual field logic.
    /// </summary>
    /// <param name="model">The category model.</param>
    /// <returns>The mapped category dto.</returns>
    [UserMapping(Default = true)]
    public TagDTO TagModelToTagDto(TagModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var dto = this.TagModelToTagDtoCore(model);

        dto.TotalBooks = model.Books.Count;

        return dto;
    }

    /// <summary>
    /// Map a tag model to a tag dto.
    /// </summary>
    /// <param name="model">The tag model.</param>
    /// <returns>The tag dto.</returns>
    [MapperIgnoreSource(nameof(TagModel.Books))]
    [MapperIgnoreSource(nameof(TagModel.CreatedAt))]
    [MapperIgnoreSource(nameof(TagModel.UpdatedAt))]
    [MapperIgnoreTarget(nameof(TagDTO.TotalBooks))]
    public partial TagDTO TagModelToTagDtoCore(TagModel model);

    /// <summary>
    /// Map a create tag dto to a tag model.
    /// </summary>
    /// <param name="dto">The create tag dto.</param>
    /// <returns>The tag model.</returns>
    [MapperIgnoreTarget(nameof(TagModel.Id))]
    [MapperIgnoreTarget(nameof(TagModel.Books))]
    [MapperIgnoreTarget(nameof(TagModel.CreatedAt))]
    [MapperIgnoreTarget(nameof(TagModel.UpdatedAt))]
    public partial TagModel CreateTagDtoToTagModel(CreateTagDTO dto);

    /// <summary>
    /// Map a tag dto to a create tag dto.
    /// </summary>
    /// <param name="dto">The tag dto.</param>
    /// <returns>The create tag dto.</returns>
    [MapperIgnoreSource(nameof(TagDTO.Id))]
    [MapperIgnoreSource(nameof(TagDTO.TotalBooks))]
    public partial CreateTagDTO TagDtoToCreateTagDto(TagDTO dto);
}
