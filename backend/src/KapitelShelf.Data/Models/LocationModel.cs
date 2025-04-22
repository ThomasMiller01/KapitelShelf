namespace KapitelShelf.Data.Models
{
    public class LocationModel
    {
        public Guid Id { get; set; }

        public LocationTypeEnum Type { get; set; }

        public string? Url { get; set; }
        public FileInfoModel? FileInfo { get; set; }
    }
}
