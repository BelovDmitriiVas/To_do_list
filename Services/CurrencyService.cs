using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace To_Do_List_Prod.Services

{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly Dictionary<string, decimal> _previousRates = new();

        public CurrencyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["CurrencySettings:ApiKey"];
            _baseUrl = configuration["CurrencySettings:BaseUrl"];
        }

        public async Task<Dictionary<string, (decimal Rate, string ChangeColor)>> GetExchangeRatesAsync()
        {
            var currencies = new Dictionary<string, string>
            {
                { "USD", "EUR" }
            };

            var exchangeRates = new Dictionary<string, (decimal, string)>();

            foreach (var (fromCurrency, toCurrency) in currencies)
            {
                decimal rate = await FetchExchangeRate(fromCurrency, toCurrency);
                string changeColor = DetermineColorChange(fromCurrency, rate);

                exchangeRates[$"1 {fromCurrency} = {rate:F2} {toCurrency}"] = (rate, changeColor);
            }

            return exchangeRates;
        }

        private async Task<decimal> FetchExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                string url = $"{_baseUrl}?function=CURRENCY_EXCHANGE_RATE&from_currency={fromCurrency}&to_currency={toCurrency}&apikey={_apiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Error: {response.StatusCode} for {fromCurrency}/{toCurrency}");
                    return 0;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response for {fromCurrency}/{toCurrency}: {jsonResponse}"); // Лог API-ответа

                using var jsonDoc = JsonDocument.Parse(jsonResponse);

                if (jsonDoc.RootElement.TryGetProperty("Realtime Currency Exchange Rate", out var exchangeRateElement) &&
                    exchangeRateElement.TryGetProperty("5. Exchange Rate", out var rateElement) &&
                    decimal.TryParse(rateElement.GetString(), out decimal rate))
                {
                    return rate;
                }

                Console.WriteLine($"Failed to parse exchange rate for {fromCurrency}/{toCurrency}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception fetching exchange rate: {ex.Message}");
                return 0; // Возвращаем 0, чтобы сервер не падал
            }
        }



        private string DetermineColorChange(string currency, decimal currentRate)
        {
            if (_previousRates.TryGetValue(currency, out decimal previousRate))
            {
                if (currentRate < previousRate) return "green";
                if (currentRate > previousRate) return "red";
            }

            _previousRates[currency] = currentRate;
            return "black";
        }
    }
}
