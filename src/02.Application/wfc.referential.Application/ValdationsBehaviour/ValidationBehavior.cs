using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace wfc.referential.Application.ValdationsBehaviour;

public class ValidationBehavior<TRequest, TResponse>
(IEnumerable<IValidator<TRequest>> validators,
 ILogger<ValidationBehavior<TRequest, TResponse>> logger,
 IOptions<SpecialCharacterValidationOptions> options)
: IPipelineBehavior<TRequest, TResponse>
where TRequest : ICommand<TResponse>
{
    private readonly SpecialCharacterValidationOptions _options = options.Value;
    private readonly Lazy<Regex> _forbiddenCharsRegex = new(() =>
        new Regex(options.Value.ForbiddenCharactersPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant));

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var allErrors = new List<ValidationFailure>();
        if (_options.IsEnabled)
        {
            var specialCharErrors = ValidateSpecialCharacters(request);
            allErrors.AddRange(specialCharErrors);
        }
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var fluentValidationErrors = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            allErrors.AddRange(fluentValidationErrors);
        }

        if (allErrors.Count != 0)
        {
            throw new InputValidationException(allErrors);
        }

        return await next();
    }

    // <summary>
    /// Automatic special character validation on all string properties
    /// </summary>
    private List<ValidationFailure> ValidateSpecialCharacters(TRequest request)
    {
        var errors = new List<ValidationFailure>();

        try
        {
            ValidateObjectRecursive(request, "", errors);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error during special character validation for {RequestType}", typeof(TRequest).Name);
        }

        return errors;
    }

    private void ValidateObjectRecursive(object obj, string path, List<ValidationFailure> errors)
    {
        if (obj == null) return;

        var type = obj.GetType();

        // If it's a string, validate directly
        if (type == typeof(string))
        {
            ValidateString((string)obj, path, errors);
            return;
        }

        // If it's a primitive type, skip
        if (type.IsPrimitive || type.IsEnum || type == typeof(DateTime) ||
            type == typeof(Guid) || type == typeof(decimal) || type == typeof(DateOnly) ||
            type == typeof(TimeOnly) || type == typeof(DateTimeOffset))
            return;

        // If it's a collection
        if (obj is IEnumerable enumerable && type != typeof(string))
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                var itemPath = string.IsNullOrEmpty(path) ? $"[{index}]" : $"{path}[{index}]";
                ValidateObjectRecursive(item, itemPath, errors);
                index++;
            }
            return;
        }

        // If it's a complex object, validate its properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (!property.CanRead) continue;

            // Exclude sensitive properties according to configuration
            if (IsPropertyExcluded(property.Name))
            {
                continue;
            }

            var propertyName = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";

            try
            {
                var value = property.GetValue(obj);
                ValidateObjectRecursive(value, propertyName, errors);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Error validating property {PropertyName}", propertyName);
            }
        }
    }

    private bool IsPropertyExcluded(string propertyName)
    {
        return _options.ExcludedProperties.Any(excluded =>
            propertyName.Contains(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private void ValidateString(string value, string propertyName, List<ValidationFailure> errors)
    {
        if (string.IsNullOrEmpty(value)) return;

        var matches = _forbiddenCharsRegex.Value.Matches(value);
        if (matches.Count > 0)
        {
            string errorMessage;

            if (_options.EnableDetailedErrorMessages)
            {
                var foundChars = matches.Cast<Match>()
                    .Select(m => GetDisplayCharacter(m.Value))
                    .Distinct()
                    .OrderBy(c => c);

                var charList = string.Join(", ", foundChars);
                errorMessage = $"Contains forbidden characters: {charList}";
            }
            else
            {
                errorMessage = "Contains forbidden special characters";
            }

            // Use proper property name or fallback
            var displayPropertyName = string.IsNullOrEmpty(propertyName) ? "Field" : propertyName;

            errors.Add(new ValidationFailure(displayPropertyName, errorMessage));
        }
    }

    private static string GetDisplayCharacter(string character)
    {
        return character switch
        {
            "\r" => "\\r",
            "\n" => "\\n",
            "\t" => "\\t",
            " " => "space",
            _ => character
        };
    }
}
