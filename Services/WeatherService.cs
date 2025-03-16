using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace To_Do_List_Prod.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Dictionary<string, decimal>> GetCurrentWeatherAsync()
        {
            var cities = new Dictionary<string, (double lat, double lon)>
            {
                { "Тверь", (56.8584, 35.9006) },
                { "Баку", (40.4093, 49.8671) }
            };

            var weatherData = new Dictionary<string, decimal>();

            foreach (var city in cities)
            {
                decimal temperature = await FetchCurrentTemperature(city.Value.lat, city.Value.lon);
                weatherData[city.Key] = temperature;
            }

            return weatherData;
        }

        private async Task<decimal> FetchCurrentTemperature(double latitude, double longitude)
        {
            try
            {
                string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true&timezone=auto";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(jsonResponse);

                if (jsonDoc.RootElement.TryGetProperty("current_weather", out var weatherElement) &&
                    weatherElement.TryGetProperty("temperature", out var tempElement) &&
                    decimal.TryParse(tempElement.GetRawText(), out decimal temperature))
                {
                    return temperature;
                }

                Console.WriteLine($"Failed to parse temperature for {latitude}, {longitude}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Weather API error: {ex.Message}");
                return 0;
            }
        }
    }
}
