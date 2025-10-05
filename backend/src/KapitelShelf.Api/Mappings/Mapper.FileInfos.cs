// <copyright file="Mapper.FileInfos.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Data.Models;
using Riok.Mapperly.Abstractions;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The fileinfos mapper.
/// </summary>
public sealed partial class Mapper
{
    /// <summary>
    /// Map a fileinfo model to a fileinfo dto.
    /// </summary>
    /// <param name="model">The fileinfo model.</param>
    /// <returns>The fileinfo dto.</returns>
    public partial FileInfoDTO FileInfoModelToFileInfoDto(FileInfoModel model);

    /// <summary>
    /// Map a fileinfo dto to a fileinfo model.
    /// </summary>
    /// <param name="dto">The fileinfo dto.</param>
    /// <returns>The fileinfo model.</returns>
    [MapperIgnoreSource(nameof(FileInfoDTO.FileName))]
    public partial FileInfoModel FileInfoDtoToFileInfoModel(FileInfoDTO dto);
}
