using wfc.referential.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Interfaces;

namespace wfc.referential.Infrastructure.CachingManagement.Commands
{
    public class RefreshReferentielCommand
    {
        public static async Task ExecuteAsync(ApplicationDbContext context, ICacheService cacheService)
        {
            var referentiel = context.Countries
                .Include(p => p.Regions)
                .ToList();

            await cacheService.SetAsync("Referentiel", referentiel, TimeSpan.FromHours(24));
            Console.WriteLine("✅ Référentiel mis à jour dans Redis.");
        }
    }
}
