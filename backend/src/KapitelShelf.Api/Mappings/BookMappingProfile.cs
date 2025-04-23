// <copyright file="BookMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

/// <summary>
/// The book mapping profile.
/// </summary>
public class BookMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookMappingProfile"/> class.
    /// </summary>
    public BookMappingProfile()
    {
        CreateMap<BookModel, BookDTO>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                src.Categories
                    .Select(x => x.Category)
                    .ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.Tags
                    .Select(x => x.Tag)
                    .ToList()))
        .ReverseMap()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                src.Categories
                    .Select(x => new BookCategoryModel { CategoryId = x.Id })
                    .ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.Tags
                    .Select(x => new BookTagModel { TagId = x.Id })
                    .ToList()));
    }
}
