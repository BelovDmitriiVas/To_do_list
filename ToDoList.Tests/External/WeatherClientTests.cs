using Xunit;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using To_Do_List_Prod.Services;

namespace ToDoList.Tests.External;

public class WeatherServiceTests
{
    private static HttpClient CreateHttpClient(string content, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = status,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object);
    }

    private static string BuildWeatherJson(decimal temperature) =>
        $$"""
        {
          "current_weather": {
            "temperature": {{temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}}
          }
        }
        """;

    [Fact]
    public async Task GetCurrentWeatherAsync_Should_Return_Temperatures()
    {
        var httpClient = CreateHttpClient(BuildWeatherJson(25.4m));
        var service = new WeatherService(httpClient);

        var result = await service.GetCurrentWeatherAsync();

        result.Should().Contain(new[]
        {
            new KeyValuePair<string, decimal>("Тверь", 25.4m),
            new KeyValuePair<string, decimal>("Баку", 25.4m)
        });
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_Should_Return_Zero_On_InvalidJson()
    {
        var httpClient = CreateHttpClient("{}");
        var service = new WeatherService(httpClient);

        var result = await service.GetCurrentWeatherAsync();

        result.Values.Should().AllBeEquivalentTo(0);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_Should_Return_Zero_On_HttpError()
    {
        var httpClient = CreateHttpClient("Error", HttpStatusCode.InternalServerError);
        var service = new WeatherService(httpClient);

        var result = await service.GetCurrentWeatherAsync();

        result.Values.Should().AllBeEquivalentTo(0);
    }
}
