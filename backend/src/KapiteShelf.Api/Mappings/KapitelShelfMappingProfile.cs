using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings
{
    public class KapitelShelfMappingProfile : Profile
    {
        public KapitelShelfMappingProfile()
        {
            CreateMap<BookModel, BookDTO>()
                .ReverseMap();

            CreateMap<AuthorModel, AuthorDTO>()
                .ReverseMap();

            CreateMap<SeriesModel, SeriesDTO>()
                .ReverseMap();

            CreateMap<FileInfoModel, FileInfoDTO>()
                .ReverseMap();

            CreateMap<LocationTypeEnum, LocationTypeDTO>()
                .ReverseMap();

            CreateMap<LocationModel, LocationDTO>()
                .ReverseMap();

            CreateMap<CategoryModel, CategoryDTO>()
                .ReverseMap();

            CreateMap<TagModel, TagDTO>()
                .ReverseMap();
        }
    }
}
