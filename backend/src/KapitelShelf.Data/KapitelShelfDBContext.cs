using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Data;

public class KapitelShelfDBContext : DbContext
{
    public KapitelShelfDBContext(DbContextOptions<KapitelShelfDBContext> options) : base(options) { }
}
