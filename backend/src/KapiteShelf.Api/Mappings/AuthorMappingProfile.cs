using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

public class AuthorMappingProfile : Profile
{
    public AuthorMappingProfile()
    {
        CreateMap<AuthorModel, AuthorDTO>()
            .ReverseMap();
    }
}
