using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

public class BookMappingProfile : Profile
{
    public BookMappingProfile()
    {
        CreateMap<BookModel, BookDTO>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                src.Categories
                    .Select(x => x.Category)
                    .ToList()
            ))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.Tags
                    .Select(x => x.Tag)
                    .ToList()
            ))
        .ReverseMap()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                src.Categories
                    .Select(x => new BookCategoryModel { CategoryId = x.Id })
                    .ToList()
            ))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                src.Tags
                    .Select(x => new BookTagModel { TagId = x.Id })
                    .ToList()
            ));
    }
}
