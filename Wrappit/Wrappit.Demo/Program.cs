using Wrappit.Configuration;
using Wrappit.Demo.Listeners;
using Wrappit.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// It is also possible to omit the wrappit options if the corresponding environment variables are set.
builder.Services.AddWrappit(new WrappitOptions { DeliveryLimit = 2 });
builder.Services.AddTransient<IExample, Example>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
