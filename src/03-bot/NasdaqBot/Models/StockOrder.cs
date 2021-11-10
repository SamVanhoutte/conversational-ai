namespace NasdaqBot.Models
{
    public class StockOrder
    {
        public string OrderNumber { get; set; }
        public string StockSymbol { get; set; }
        public string OrderType { get; set; }
        public double Costs { get; set; }
    }
}