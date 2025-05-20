using System.Reflection;
using BuildingBlocks.Core.CoreServices;
using FastEndpoints;
using FastEndpoints.Swagger;
using wfc.referential.API;
using wfc.referential.Application;
using wfc.referential.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.AddServiceDefaults("ReferentialApi");

builder.Services.AddAuthorization();

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(config =>
{
    config.AutoTagPathSegmentIndex = 0; // turn off path-based tagging
    
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{

    var appAssembly = Assembly.Load("wfc.referential.Application");
    var xmlName2 = $"{appAssembly.GetName().Name}.xml";
    var xmlPath2 = Path.Combine(AppContext.BaseDirectory, xmlName2);
    if (File.Exists(xmlPath2))
        options.IncludeXmlComments(xmlPath2);
});

builder
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);
var app = builder.Build();

app.UseHttpsRedirection();

//app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WFC Referential API v1"));
}

app.UseApiServices();

app.Run();

public partial class Program { }