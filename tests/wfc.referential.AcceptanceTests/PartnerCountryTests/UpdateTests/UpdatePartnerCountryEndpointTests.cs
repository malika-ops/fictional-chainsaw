using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnerCountryTests.UpdateTests;

public class UpdatePartnerCountryEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private readonly Mock<IPartnerCountryRepository> _pcRepo = new();
    private readonly Mock<IPartnerRepository> _partnerRepo = new();
    private readonly Mock<ICountryRepository> _countryRepo = new();

    public UpdatePartnerCountryEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IPartnerCountryRepository>();
                s.RemoveAll<IPartnerRepository>();
                s.RemoveAll<ICountryRepository>();
                s.RemoveAll<ICacheService>();

                _pcRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

                s.AddSingleton(_pcRepo.Object);
                s.AddSingleton(_partnerRepo.Object);
                s.AddSingleton(_countryRepo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static PartnerCountry Make(Guid id, Guid partner, Guid country, bool enabled = true)
    {
        var pc = PartnerCountry.Create(
                    PartnerCountryId.Of(id),
                    PartnerId.Of(partner),
                    CountryId.Of(country));

        if (!enabled) pc.Disable();
        return pc;
    }


    [Fact(DisplayName = "PUT /api/partner-countries/{id} → 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateSuccessful()
    {
        var id = Guid.NewGuid();
        var oldP = Guid.NewGuid();
        var oldC = Guid.NewGuid();
        var newP = Guid.NewGuid();
        var newC = Guid.NewGuid();

        var entity = Make(id, oldP, oldC, enabled: true);

        _pcRepo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
               .ReturnsAsync(entity);

        _partnerRepo.Setup(r => r.GetByIdAsync(PartnerId.Of(newP), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.PartnerAggregate.Partner)) as Domain.PartnerAggregate.Partner);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(newC), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.Countries.Country)) as Domain.Countries.Country);

        _pcRepo.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<PartnerCountry, bool>>>(),
                        It.IsAny<CancellationToken>()))
               .ReturnsAsync((PartnerCountry?)null);   

        var payload = new
        {
            PartnerCountryId = id,
            PartnerId = newP,
            CountryId = newC,
            IsEnabled = false
        };

        var res = await _client.PutAsJsonAsync($"/api/partner-countries/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        entity.PartnerId.Value.Should().Be(newP);
        entity.CountryId.Value.Should().Be(newC);
        entity.IsEnabled.Should().BeFalse();

        _pcRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "PUT /api/partner-countries/{id} → 404 when PartnerCountry missing")]
    public async Task Put_ShouldReturn404_WhenPartnerCountryMissing()
    {
        var id = Guid.NewGuid();

        _pcRepo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
               .ReturnsAsync((PartnerCountry?)null);

        var payload = new
        {
            PartnerCountryId = id,
            PartnerId = Guid.NewGuid(),
            CountryId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync($"/api/partner-countries/{id}", payload);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact(DisplayName = "PUT /api/partner-countries/{id} → 404 when Partner missing")]
    public async Task Put_ShouldReturn404_WhenPartnerMissing()
    {
        var id = Guid.NewGuid();
        var partId = Guid.NewGuid();
        var ctryId = Guid.NewGuid();

        var entity = Make(id, Guid.NewGuid(), Guid.NewGuid());

        _pcRepo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
               .ReturnsAsync(entity);

        _partnerRepo.Setup(r => r.GetByIdAsync(PartnerId.Of(partId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Domain.PartnerAggregate.Partner?)null);    

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(ctryId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.Countries.Country)) as Domain.Countries.Country);

        var payload = new { PartnerCountryId = id, PartnerId = partId, CountryId = ctryId };

        var res = await _client.PutAsJsonAsync($"/api/partner-countries/{id}", payload);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _pcRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PUT /api/partner-countries/{id} → 404 when Country missing")]
    public async Task Put_ShouldReturn404_WhenCountryMissing()
    {
        var id = Guid.NewGuid();
        var partId = Guid.NewGuid();
        var ctryId = Guid.NewGuid();

        var entity = Make(id, Guid.NewGuid(), Guid.NewGuid());

        _pcRepo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
               .ReturnsAsync(entity);

        _partnerRepo.Setup(r => r.GetByIdAsync(PartnerId.Of(partId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.PartnerAggregate.Partner)) as Domain.PartnerAggregate.Partner);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(ctryId), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Domain.Countries.Country?)null);

        var payload = new { PartnerCountryId = id, PartnerId = partId, CountryId = ctryId };

        var res = await _client.PutAsJsonAsync($"/api/partner-countries/{id}", payload);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _pcRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PUT /api/partner-countries/{id} → 409 when duplicate link exists")]
    public async Task Put_ShouldReturn409_WhenDuplicateExists()
    {
        var idTarget = Guid.NewGuid();
        var partner = Guid.NewGuid();
        var country = Guid.NewGuid();

        var target = Make(idTarget, partner, country);
        var existing = Make(Guid.NewGuid(), partner, country);

        _pcRepo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(idTarget), It.IsAny<CancellationToken>()))
               .ReturnsAsync(target);

        _partnerRepo.Setup(r => r.GetByIdAsync(PartnerId.Of(partner), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.PartnerAggregate.Partner)) as Domain.PartnerAggregate.Partner);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(country), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.Countries.Country)) as Domain.Countries.Country);

        _pcRepo.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<PartnerCountry, bool>>>(),
                        It.IsAny<CancellationToken>()))
               .ReturnsAsync(existing);

        var payload = new { PartnerCountryId = idTarget, PartnerId = partner, CountryId = country };

        var res = await _client.PutAsJsonAsync($"/api/partner-countries/{idTarget}", payload);
        res.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _pcRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "PUT /api/partner-countries/{id} → 200 when disabling link")]
    public async Task Put_ShouldReturn200_WhenDisabling()
    {
        var id = Guid.NewGuid();
        var pid = Guid.NewGuid();
        var cid = Guid.NewGuid();

        var link = Make(id, pid, cid, enabled: true);

        _pcRepo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
               .ReturnsAsync(link);

        _partnerRepo.Setup(r => r.GetByIdAsync(PartnerId.Of(pid), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.PartnerAggregate.Partner)) as Domain.PartnerAggregate.Partner);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(cid), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.Countries.Country)) as Domain.Countries.Country);

        _pcRepo.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<PartnerCountry, bool>>>(),
                        It.IsAny<CancellationToken>()))
               .ReturnsAsync((PartnerCountry?)null);

        var payload = new { PartnerCountryId = id, PartnerId = pid, CountryId = cid, IsEnabled = false };

        var res = await _client.PutAsJsonAsync($"/api/partner-countries/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        link.IsEnabled.Should().BeFalse();
    }


    [Fact(DisplayName = "PUT /api/partner-countries/{id} → 200 when same Partner+Country")]
    public async Task Put_ShouldReturn200_WhenSameMapping()
    {
        var id = Guid.NewGuid();
        var pid = Guid.NewGuid();
        var cid = Guid.NewGuid();

        var link = Make(id, pid, cid);

        _pcRepo.Setup(r => r.GetByIdAsync(PartnerCountryId.Of(id), It.IsAny<CancellationToken>()))
               .ReturnsAsync(link);

        _partnerRepo.Setup(r => r.GetByIdAsync(PartnerId.Of(pid), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.PartnerAggregate.Partner)) as Domain.PartnerAggregate.Partner);

        _countryRepo.Setup(r => r.GetByIdAsync(CountryId.Of(cid), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(FormatterServices.GetUninitializedObject(typeof(Domain.Countries.Country)) as Domain.Countries.Country);

        _pcRepo.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<PartnerCountry, bool>>>(),
                        It.IsAny<CancellationToken>()))
               .ReturnsAsync(link);  

        var payload = new { PartnerCountryId = id, PartnerId = pid, CountryId = cid };

        var res = await _client.PutAsJsonAsync($"/api/partner-countries/{id}", payload);
        var ok = await res.Content.ReadFromJsonAsync<bool>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        _pcRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "PUT /api/partner-countries/{id} → 400 when PartnerCountryId empty")]
    public async Task Put_ShouldReturn400_WhenIdEmpty()
    {
        var payload = new
        {
            PartnerCountryId = Guid.Empty,
            PartnerId = Guid.NewGuid(),
            CountryId = Guid.NewGuid()
        };

        var res = await _client.PutAsJsonAsync(
            "/api/partner-countries/00000000-0000-0000-0000-000000000000",
            payload);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}