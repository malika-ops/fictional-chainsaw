namespace wfc.referential.Application.Controles.Dtos;

public record GetControleResponse
{
    /// <summary>The unique identifier for the Controle.</summary>
    /// <example>6f9619ff-8b86-d011-b42d-00cf4fc964ff</example>
    public Guid Id { get; init; }

    /// <summary>The unique code that identifies the Controle.</summary>
    /// <example>CTRL-001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>The display name of the Controle.</summary>
    /// <example>Identity Check Control</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Whether the Controle is currently enabled.</summary>
    /// <example>true</example>
    public bool IsEnabled { get; init; }

    /// <summary>Creation date.</summary>
    /// <example>2025-06-23T10:30:00+00:00</example>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Last modification date.</summary>
    /// <example>2025-06-23T14:45:00+00:00</example>
    public DateTimeOffset LastModified { get; init; }

    /// <summary>Created by</summary>
    /// <example>john doe</example>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>Last modified by</summary>
    /// <example>jane smith</example>
    public string LastModifiedBy { get; init; } = string.Empty;
}
