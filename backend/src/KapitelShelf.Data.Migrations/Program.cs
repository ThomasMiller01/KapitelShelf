using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<KapitelShelfDBContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=kapitelshelf;Username=kapitelshelf;Password=kapitelshelf")
            .Options;

using var context = new KapitelShelfDBContext(options);

context.Database.Migrate();

Console.WriteLine("Migrations applied.");