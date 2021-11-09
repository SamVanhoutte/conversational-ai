using System;

namespace NasdaqBot.Models
{
    public class BuyStockRequest
    {
        public int Amount { get; set; }
        public string StockSymbol { get; set; }
        public double OrderLimit { get; set; }
    }
}