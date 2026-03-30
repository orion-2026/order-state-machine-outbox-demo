using OrderStateMachineOutboxDemo.Infrastructure;
using OrderStateMachineOutboxDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<InMemoryOrderStore>();
builder.Services.AddSingleton<OrderStateMachine>();
builder.Services.AddSingleton<OrderApplicationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
