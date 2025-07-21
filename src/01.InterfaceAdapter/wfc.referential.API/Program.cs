using System.Reflection;
using System.Text.Json;
using BuildingBlocks.Core.CoreServices;
using Serilog;
using wfc.referential.API;
using wfc.referential.Application;
using wfc.referential.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults("ReferentialApi");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ReferentialApi")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .CreateLogger();


builder.Host.UseSerilog();
builder.Services.AddAuthorization();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Include XML comments from API assembly for Minimal APIs documentation
    var apiAssembly = Assembly.GetExecutingAssembly();
    var xmlName1 = $"{apiAssembly.GetName().Name}.xml";
    var xmlPath1 = Path.Combine(AppContext.BaseDirectory, xmlName1);
    if (File.Exists(xmlPath1))
       { options.IncludeXmlComments(xmlPath1, includeControllerXmlComments: true); 
        options.OperationFilter<XmlCommentsForAsParametersOperationFilter>(xmlPath1);
    }

    // Include XML comments from Application assembly
    var appAssembly = Assembly.Load("wfc.referential.Application");
    var xmlName2 = $"{appAssembly.GetName().Name}.xml";
    var xmlPath2 = Path.Combine(AppContext.BaseDirectory, xmlName2);
    if (File.Exists(xmlPath2))
       { options.IncludeXmlComments(xmlPath2);
        options.OperationFilter<XmlCommentsForAsParametersOperationFilter>(xmlPath2);
    }
    options.SupportNonNullableReferenceTypes();
    options.EnableAnnotations();
});

builder
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
//app.UseAuthentication();
app.UseAuthorization();

// Map Minimal API endpoints 
app.MapEndpoints();


    app.UseSwagger(); // Add this for Minimal APIs
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WFC Referential API v1");
        c.RoutePrefix = "swagger";
    });


app.UseApiServices();
app.Run();

public partial class Program { }