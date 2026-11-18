using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FinanCore.API.IntegrationTests;

public class ApiSmokeTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_Root_SemAutenticacao_DeveRetornar401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
