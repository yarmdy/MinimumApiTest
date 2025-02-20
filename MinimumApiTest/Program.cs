using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IClassRequestHandlerFactory, ClassRequestHandlerFactory>();
builder.Services.AddTransient<HomeClass>();
builder.Services.AddTransient<StudentClass>();
builder.Services.AddScoped<Stopwatch>();

// Add services to the container.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.MapGet("help",()=>Results.Content("°ïÖú"));
app.MapClass<HomeClass>("home/{action}");
app.MapClass<StudentClass>("student/{action}/{id?}");
app.Run();
