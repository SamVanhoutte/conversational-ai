namespace NasdaqBot.Models
{
    public abstract class StockOrderRequest
    {
        public int Amount { get; set; }
        public string StockSymbol { get; set; }
        public decimal OrderLimit { get; set; }
    }
}