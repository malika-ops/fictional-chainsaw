using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Taxes.Dtos;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.TaxAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TaxTests.UpdateTests;

public class UpdateTaxEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITaxRepository> _repoMock = new();
    private const string BaseUrl = "api/taxes";


    public UpdateTaxEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaxRepository>();
                services.RemoveAll<ICacheService>();

                // default noop for Update
                _repoMock
                    .Setup(r => r.UpdateTaxAsync(It.IsAny<Tax>(),
                                                          It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }


    private static Tax DummyTax(Guid id,string code, string description) =>
        Tax.Create(TaxId.Of(id), code, $"{code}AR", $"{code}EN", description, 43, 20);


    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldTax = DummyTax(id, "VAT_DE", "Germany VAT - Standard Rate");

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldTax);

        Tax? updated = null;
        _repoMock.Setup(r => r.UpdateTaxAsync(oldTax, It.IsAny<CancellationToken>()))
                 .Callback<Tax, CancellationToken>((rg, _) => updated = rg)
                 .Returns(Task.CompletedTask);

        var payload = new UpdateTaxRequest
        {
            Code = "testAAB",
            CodeEn = "TestAABEN",
            CodeAr = "testAABAR",
            Description = "Value Added Tax applicable to goods and services",
            FixedAmount = 43,
            Value = 15,
            IsEnabled = true,
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("testAAB");
        updated.CodeEn.Should().Be("TestAABEN");

        _repoMock.Verify(r => r.UpdateTaxAsync(It.IsAny<Tax>(),It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 400 when Code is missing")]
    public async Task Put_ShouldReturn400_WhenCodeMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new UpdateTaxRequest
        {
           CodeEn= "Service Tax",
           CodeAr= "ضريبة الخدمات",
           Description= "Applies to services provided within the country.",
           FixedAmount= 43,
           Value= 10.0,
           IsEnabled= true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required.");

        _repoMock.Verify(r => r.UpdateTaxAsync(It.IsAny<Tax>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = $"PUT {BaseUrl}/id returns 400 when new code already exists")]
    public async Task Put_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = DummyTax(Guid.NewGuid(),"GST_CA", "Canada GST - Federal Goods and Services Tax");
        var target = DummyTax(id, "SALETX_CA", "US Sales Tax - California");

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("GST_CA", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // duplicate code

        var payload = new CreateTaxRequest
        {
            Code = "GST_CA",
            CodeEn = "Service Tax",
            CodeAr = "ضريبة الخدمات",
            Description = "Applies to services provided within the country.",
            FixedAmount = 43,
            Value = 10.0
        };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"{nameof(Tax)} with code : {payload.Code} already exist");

        _repoMock.Verify(r => r.UpdateTaxAsync(It.IsAny<Tax>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
