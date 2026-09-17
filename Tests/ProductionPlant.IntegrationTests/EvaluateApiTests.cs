using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace ProductionPlant.IntegrationTests;

public sealed class EvaluateApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EvaluateApiTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Evaluate_endpoint_rejects_missing_order_id()
    {
        var request = new
        {
            publisherNumber = "99999",
            orderMethod = "POD",
            items = new[]
            {
                new
                {
                    printQuantity = 10
                }
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/evaluate",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}