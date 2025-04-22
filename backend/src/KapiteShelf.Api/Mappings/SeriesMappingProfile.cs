using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

public class SeriesMappingProfile : Profile
{
    public SeriesMappingProfile()
    {
        CreateMap<SeriesModel, SeriesDTO>()
            .ReverseMap();
    }
}
