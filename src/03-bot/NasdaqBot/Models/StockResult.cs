using System;
using System.Collections.Generic;

namespace NasdaqBot.Models
{
    public class StockResult
    {
        public string StockSymbol { get; set; }
        public double Result { get; set; }
        public IDictionary<DateTime,double> ChartData { get; set; }
    }
}