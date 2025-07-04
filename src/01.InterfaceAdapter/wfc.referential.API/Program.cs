using System.Reflection;
using System.Text.Json;
using System.Xml.XPath;
using BuildingBlocks.Core.CoreServices;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using wfc.referential.API;
using wfc.referential.Application;
using wfc.referential.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults("ReferentialApi");
//Log.Logger = new LoggerConfiguration()

//    .MinimumLevel.Information()
//    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
//    .Enrich.FromLogContext()
//    .WriteTo.File("/app/logs/app-.log",

//        rollingInterval: RollingInterval.Day,

//        retainedFileCountLimit: 30,

//        shared: true,

//        flushToDiskInterval: TimeSpan.FromSeconds(1))

//    .CreateLogger();



//builder.Host.UseSerilog();
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
//app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
app.Run();

public partial class Program { }

public class XmlCommentsForAsParametersOperationFilter : IOperationFilter
{
    private readonly XPathDocument _xmlDoc;
    private readonly XPathNavigator _navigator;

    public XmlCommentsForAsParametersOperationFilter(string xmlPath)
    {
        if (File.Exists(xmlPath))
        {
            _xmlDoc = new XPathDocument(xmlPath);
            _navigator = _xmlDoc.CreateNavigator();
        }
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null || _navigator == null)
            return;

        // Trouver dans la méthode si un paramètre est un DTO avec [AsParameters]
        foreach (var parameterInfo in context.MethodInfo.GetParameters())
        {
            var paramType = parameterInfo.ParameterType;

            // Vérifier que le paramètre est un "record" ou class avec propriétés
            if (!paramType.IsClass && !paramType.IsValueType)
                continue;

            foreach (var openApiParam in operation.Parameters)
            {
                // Chercher la propriété qui correspond au paramètre query
                var property = paramType.GetProperties()
                    .FirstOrDefault(p => string.Equals(p.Name, openApiParam.Name, StringComparison.OrdinalIgnoreCase));

                if (property == null) continue;

                // Extraire le commentaire XML
                var xmlMemberName = $"P:{property.DeclaringType.FullName}.{property.Name}";
                var xpath = $"/doc/members/member[@name='{xmlMemberName}']/summary";

                var node = _navigator.SelectSingleNode(xpath);
                if (node != null)
                {
                    var desc = node.Value.Trim();
                    if (!string.IsNullOrWhiteSpace(desc))
                    {
                        openApiParam.Description = desc;
                    }
                }
            }
        }
    }
}