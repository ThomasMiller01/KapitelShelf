// <copyright file="BookMappingProfile.cs" company="KapitelShelf">
// Copyright (c) KapitelShelf. All rights reserved.
// </copyright>

using AutoMapper;
using KapitelShelf.Api.DTOs.Book;
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

            // map series from the model if set, otherwise just take the seriesId from the model
            .ForMember(dest => dest.Series, opt => opt.MapFrom(src =>
                src.Series ?? new SeriesModel { Id = src.SeriesId }));

        CreateMap<CreateBookDTO, BookModel>()
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.Series, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore())
            .ForMember(dest => dest.Cover, opt => opt.Ignore());

        CreateMap<BookDTO, CreateBookDTO>();
    }
}
