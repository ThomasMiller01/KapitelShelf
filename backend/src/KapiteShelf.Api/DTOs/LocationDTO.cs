using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.DTOs
{
    public class LocationDTO
    {
        public Guid Id { get; set; }

        public LocationTypeDTO Type { get; set; }

        public string? Url { get; set; }
        public FileInfoDTO? FileInfo { get; set; }
    }
}
