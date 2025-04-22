using KapitelShelf.Data.Models;

namespace KapitelShelf.Api.DTOs
{
    public class BookDTO
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public DateTime? ReleaseDate { get; set; }

        public int? PageNumber { get; set; }

        public SeriesDTO? Series { get; set; }
        public int? SeriesNumber = null;

        public AuthorDTO? Author { get; set; }

        public IList<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public IList<TagDTO> Tags { get; set; } = new List<TagDTO>();

        public FileInfoDTO? Cover { get; set; }
        public LocationDTO? Location { get; set; }
    }
}
