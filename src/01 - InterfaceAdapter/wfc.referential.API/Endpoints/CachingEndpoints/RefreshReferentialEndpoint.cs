using FastEndpoints;
using wfc.referential.Infrastructure.Data;
using wfc.referential.Infrastructure.CachingManagement.Commands;
using BuildingBlocks.Application.Interfaces;

namespace wfc.referential.API.Endpoints.CachingEndpoints;

public class RefreshReferentialEndpoint(ICacheService _cacheService, ApplicationDbContext _context, ILogger<RefreshReferentialEndpoint> _logger) : 
    EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/refreshReferential");
        AllowAnonymous();
        Options(o => o.WithTags(EndpointGroups.CachingReferential));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            await RefreshReferentielCommand.ExecuteAsync(_context, _cacheService);
            _logger.LogInformation("Cache référentiel mis à jour manuellement.");
            var Response = new RefreshResponse
            {
                Message = "Cache référentiel mis à jour avec succès.",
                Timestamp = DateTime.UtcNow
            };

            await SendAsync(Response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erreur lors du rafraîchissement du cache : {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }

    public class RefreshResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Error { get; set; }
    }
}