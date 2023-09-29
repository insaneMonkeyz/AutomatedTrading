namespace TradingConcepts.SecuritySpecifics
{
    public class SecurityDescription
    {
        public virtual Type SecurityType => typeof(ISecurity);

        public required string Ticker { get; init; }

        private string? _stringRepresentation;
        public override string ToString()
        {
            return _stringRepresentation ??= $"{SecurityType.Name} {Ticker}";
        }
    }
}
