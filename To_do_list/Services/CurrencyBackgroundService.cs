using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace To_Do_List_Prod.Services
{
    public class CurrencyBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CurrencyBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var currencyService = scope.ServiceProvider.GetRequiredService<CurrencyService>();
                await currencyService.GetExchangeRatesAsync();

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}