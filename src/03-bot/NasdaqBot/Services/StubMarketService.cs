using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using NasdaqBot.Models;

namespace NasdaqBot.Services
{
    public class StubMarketService : IMarketService
    {
        public Task<StockResult> GetStockResultAsync(string stockSymbol)
        {
            var rnd = new Random();
            var result = rnd.NextDouble()*5;
            if (rnd.Next(0, 2) == 0) result = -result;
            return Task.FromResult(new StockResult
            {
                Result = result,
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
                Costs = (decimal)rnd.NextDouble() * 40, OrderNumber = $"Order{rnd.Next(100, 999)}", OrderType = "Buy",
                Amount = request.Amount, Limit = request.OrderLimit, 
                StockSymbol = request.StockSymbol
            });
        }
    }
}