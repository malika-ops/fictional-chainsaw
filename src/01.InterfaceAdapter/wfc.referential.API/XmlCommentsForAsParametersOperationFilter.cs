using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace wfc.referential.API;

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
