using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace KapitelShelf.Data.Migrations;

public class KapitelShelfDesignTimeFactory : IDesignTimeDbContextFactory<KapitelShelfDBContext>
{
    public KapitelShelfDBContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql("Host=localhost;Database=kapitelshelf;Username=kapitelshelf;Password=kapitelshelf",
            b => b.MigrationsAssembly("KapitelShelf.Data.Migrations"))
            .Options;

        return new KapitelShelfDBContext(options);
    }
}
