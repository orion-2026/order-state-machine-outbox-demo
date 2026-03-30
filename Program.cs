using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OrderStateMachineOutboxDemo.Infrastructure;
using OrderStateMachineOutboxDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

var provider = builder.Configuration["DATABASE_PROVIDER"]
    ?? builder.Configuration["Database:Provider"]
    ?? "Sqlite";
var connectionString = builder.Configuration["DATABASE_CONNECTION_STRING"]
    ?? builder.Configuration["Database:ConnectionString"]
    ?? "Data Source=order-demo.db";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddScoped<OrderStateMachine>();
builder.Services.AddScoped<OrderApplicationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
