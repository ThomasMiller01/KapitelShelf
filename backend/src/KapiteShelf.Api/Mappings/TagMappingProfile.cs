using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

public class TagMappingProfile : Profile
{
    public TagMappingProfile()
    {
        CreateMap<TagModel, TagDTO>()
            .ReverseMap();
    }
}
