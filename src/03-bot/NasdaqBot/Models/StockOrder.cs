namespace NasdaqBot.Models
{
    public class StockOrder
    {
        public string OrderNumber { get; set; }
        public string StockSymbol { get; set; }
        public string OrderType { get; set; }
        public int Amount { get; set; }
        public decimal Limit { get; set; }
        public decimal Costs { get; set; }
    }
}