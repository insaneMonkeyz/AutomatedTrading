namespace Quik.EntityDataProviders.EntityDummies
{
    internal class SecurityDummy
    {
        public string? ClassCode;
        public string? Ticker;

        public bool HasData 
        {
            get => !(string.IsNullOrEmpty(Ticker) || string.IsNullOrEmpty(ClassCode));
        }

        public override string? ToString()
        {
            return string.IsNullOrEmpty(Ticker)
                ? null
                : string.IsNullOrEmpty(ClassCode)
                    ? Ticker
                    : $"{ClassCode}:{Ticker}";
        }
    }
}
