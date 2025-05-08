// <copyright file="FileInfoMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The fileinfo mapping profile.
/// </summary>
public class FileInfoMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileInfoMappingProfile"/> class.
    /// </summary>
    public FileInfoMappingProfile()
    {
        CreateMap<FileInfoModel, FileInfoDTO>()
            .ReverseMap();

        CreateMap<FileInfoDTO, CreateFileInfoDTO>();
    }
}
