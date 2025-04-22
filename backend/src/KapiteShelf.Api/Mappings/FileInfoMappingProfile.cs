using AutoMapper;
using KapitelShelf.Api.DTOs;
using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.Mappings;

public class FileInfoMappingProfile : Profile
{
    public FileInfoMappingProfile()
    {
        CreateMap<FileInfoModel, FileInfoDTO>()
            .ReverseMap();
    }
}
