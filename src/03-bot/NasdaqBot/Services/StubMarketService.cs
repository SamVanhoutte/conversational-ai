using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NasdaqBot.Models;

namespace NasdaqBot.Services
{
    public class StubMarketService : IMarketService
    {
        public Task<StockResult> GetStockResultAsync(string stockSymbol)
        {
            return Task.FromResult(new StockResult
            {
                Result = -0.32,
                StockSymbol = stockSymbol,
                ChartData = GenerateChartAsync()
            });
        }

        private IDictionary<DateTime, double> GenerateChartAsync()
        {
            var rnd = new Random();
            double lastPoint = rnd.NextDouble() * 300;
            var datePoints = new Dictionary<DateTime, double>();
            for (int dayBack = 31; dayBack > 0; dayBack--)
            {
                lastPoint = rnd.Next(0, 2) == 1 ? lastPoint - rnd.NextDouble() * 2 : lastPoint + rnd.NextDouble() * 2;
                datePoints.Add(DateTime.Today.AddDays(-dayBack), lastPoint);
            }

            return datePoints;
        }

        public Task<StockOrder> PlaceOrderAsync(BuyStockRequest request)
        {
            var rnd = new Random();

            return Task.FromResult<StockOrder>(new StockOrder
            {
                Costs = rnd.NextDouble() * 40, OrderNumber = $"Order{rnd.Next(100, 999)}", OrderType = "Buy",
                StockSymbol = request.StockSymbol
            });
        }
    }
}