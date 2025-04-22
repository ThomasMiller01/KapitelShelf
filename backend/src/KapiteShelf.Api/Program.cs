using KapitelShelf.Data;
using Microsoft.EntityFrameworkCore;
using NLog;

var builder = WebApplication.CreateBuilder(args);

// setup logging
LogManager
    .Setup()
    .LoadConfigurationFromFile("Properties/nlog.config");

// Add services to the container.
builder.Services.AddControllers();

// swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// database
builder.Services.AddDbContextFactory<KapitelShelfDBContext>(options =>
    options.UseNpgsql("Host=localhost;Database=kapitelshelf;Username=kapitelshelf;Password=kapitelshelf"));

// automapper
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options => { });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
