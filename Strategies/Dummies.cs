namespace Strategies
{
    public sealed class Quote
    {
        public decimal Price { get; }
        public int Size { get; }
    }
    public class Security
    {

    }
    public enum Operations
    {
        Buy,
        Sell
    }
}