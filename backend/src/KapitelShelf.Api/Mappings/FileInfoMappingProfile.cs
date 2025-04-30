// <copyright file="FileInfoMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.FileInfo;
using KapitelShelf.Api.Extensions;
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

        CreateMap<IFormFile, FileInfoDTO>()
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.FileSizeBytes, opt => opt.MapFrom(src => src.Length))
            .ForMember(dest => dest.MimeType, opt => opt.MapFrom(src => src.GetMimeType()))
            .ForMember(dest => dest.Sha256, opt => opt.Ignore());
    }
}
