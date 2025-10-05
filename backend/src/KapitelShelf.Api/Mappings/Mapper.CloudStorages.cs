// <copyright file="Mapper.CloudStorages.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using KapitelShelf.Api.DTOs.CloudStorage;
using KapitelShelf.Api.DTOs.CloudStorage.RClone;
using KapitelShelf.Data.Models.CloudStorage;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The cloudstorages mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a cloud configuration model to a cloud configuration dto.
    /// </summary>
    /// <param name="model">The cloud configuration model.</param>
    /// <returns>The cloud configuration dto.</returns>
    public partial CloudConfigurationDTO CloudConfigurationModelToCloudConfigurationDto(CloudConfigurationModel model);

    /// <summary>
    /// Map a cloudstorage model to a cloudstorage dto.
    /// </summary>
    /// <param name="model">The cloudstorage model.</param>
    /// <returns>The cloudstorage dto.</returns>
    [MapperIgnoreSource(nameof(CloudStorageModel.RCloneConfig))]
    public partial CloudStorageDTO CloudStorageModelToCloudStorageDto(CloudStorageModel model);

    /// <summary>
    /// The cloudstorage
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [MapperIgnoreTarget(nameof(CloudStorageModel.RCloneConfig))]
    public partial CloudStorageModel CloudStorageDtoToCloudStorageModel(CloudStorageDTO dto);

    /// <summary>
    /// Map a cloud type dto to a cloud type.
    /// </summary>
    /// <param name="dto">The cloud type dto.</param>
    /// <returns>The cloud type.</returns>
    public partial CloudType CloudTypeDtoToCloudType(CloudTypeDTO dto);

    /// <summary>
    /// Map a cloud type to a cloud type dto.
    /// </summary>
    /// <param name="type">The cloud type.</param>
    /// <returns>The cloud type dto.</returns>
    public partial CloudTypeDTO CloudTypeToCloudTypeDto(CloudType type);

    /// <summary>
    /// Map a rclone list json dto to a cloudstorage directory dto.
    /// </summary>
    /// <param name="dto">The rclone list json dto.</param>
    /// <returns>The cloudstorage directory dto.</returns>
    [MapperIgnoreSource(nameof(RCloneListJsonDTO.IsDir))]
    [MapperIgnoreSource(nameof(RCloneListJsonDTO.MimeType))]
    [MapperIgnoreSource(nameof(RCloneListJsonDTO.Size))]
    [MapperIgnoreSource(nameof(RCloneListJsonDTO.ModTime))]
    [MapperIgnoreTarget(nameof(CloudStorageDirectoryDTO.ModifiedTime))]
    [MapProperty(nameof(RCloneListJsonDTO.ID), nameof(CloudStorageDirectoryDTO.Id))]
    public partial CloudStorageDirectoryDTO RCloneListJsonDtoToCloudStorageDirectoryDto(RCloneListJsonDTO dto);

    /// <summary>
    /// Map a rclone list json dto to a cloudstorage directory dto.
    /// </summary>
    /// <param name="source">The rclone list json dto.</param>
    /// <param name="target">The cloudstorage directory dto.</param>
    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Mapperly User-Implemented")]
    private static void RCloneListJsonDtoToCloudStorageDirectoryDto(RCloneListJsonDTO source, CloudStorageDirectoryDTO target) => target.ModifiedTime = DateTime.Parse(source.ModTime, CultureInfo.InvariantCulture);
}
