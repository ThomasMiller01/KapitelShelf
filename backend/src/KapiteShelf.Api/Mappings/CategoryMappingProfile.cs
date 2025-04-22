using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<CategoryModel, CategoryDTO>()
            .ReverseMap();
    }
}
