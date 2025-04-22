using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

public class LocationMappingProfile : Profile
{
    public LocationMappingProfile()
    {
        CreateMap<LocationModel, LocationDTO>()
            .ReverseMap();

        CreateMap<LocationTypeEnum, LocationTypeDTO>()
            .ReverseMap();
    }
}
