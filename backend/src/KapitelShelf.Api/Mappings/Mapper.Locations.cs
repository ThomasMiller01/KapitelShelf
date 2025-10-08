// <copyright file="Mapper.Locations.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.Location;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The locations mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a create location dto to a location model.
    /// </summary>
    /// <param name="dto">The create location dto.</param>
    /// <returns>The location model.</returns>
    [MapperIgnoreTarget(nameof(LocationModel.Id))]
    [MapperIgnoreTarget(nameof(LocationModel.FileInfo))]
    public partial LocationModel CreateLocationDtoToLocationModel(CreateLocationDTO dto);

    /// <summary>
    /// Map a location model to a location dto.
    /// </summary>
    /// <param name="model">The location model.</param>
    /// <returns>The location dto.</returns>
    public partial LocationDTO LocationModelToLocationDto(LocationModel model);

    /// <summary>
    /// Map a location dto to a create location dto.
    /// </summary>
    /// <param name="dto">The location dto.</param>
    /// <returns>The create location dto.</returns>
    [MapperIgnoreSource(nameof(LocationModel.Id))]
    [MapperIgnoreSource(nameof(LocationModel.FileInfo))]
    public partial CreateLocationDTO LocationDtoToCreateLocationDto(LocationDTO dto);

    /// <summary>
    /// Map a location type to a location type dto.
    /// </summary>
    /// <param name="type">The location type.</param>
    /// <returns>The location type dto.</returns>
    public partial LocationTypeDTO LocationTypeToLocationTypeDto(LocationType type);

    /// <summary>
    /// Map a location type dto to a location type.
    /// </summary>
    /// <param name="dto">The location type dto.</param>
    /// <returns>The location type.</returns>
    public partial LocationType LocationTypeDtoToLocationType(LocationTypeDTO dto);
}
