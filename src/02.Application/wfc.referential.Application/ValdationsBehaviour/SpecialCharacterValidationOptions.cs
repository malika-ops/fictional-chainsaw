namespace wfc.referential.Application.ValdationsBehaviour;

public class SpecialCharacterValidationOptions
{
    public const string SectionName = "SpecialCharacterValidation";

    /// <summary>
    /// Regex pattern to detect forbidden characters
    /// </summary>
    public string ForbiddenCharactersPattern { get; set; } = @"[<>&""'/\\{}[\]();:=+*?%#@!$^`~|\r\n\t]";

    /// <summary>
    /// Properties to exclude from validation (sensitive properties)
    /// </summary>
    public string[] ExcludedProperties { get; set; } = { "password", "token", "signature", "hash", "key", "secret" };

    /// <summary>
    /// Show found characters in error messages
    /// </summary>
    public bool EnableDetailedErrorMessages { get; set; } = true;

    /// <summary>
    /// Enable/disable special character validation
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}