using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wfc.referential.Application.Currencies.Dtos;

public record DeleteCurrencyRequest
{
    /// <summary>
    /// The string representation of the Currency's GUID (route param).
    /// </summary>
    /// <example>6a472a58-5d05-4a1b-8b7f-58516dd614c3</example>
    public string CurrencyId { get; init; } = string.Empty;
}